using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Project.Scripts.Data;
using Project.Scripts.Utils;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Project.Scripts.Runtime.LevelGeneration
{
    [DisallowMultipleComponent]
    public class LevelGenerationManager: MonoBehaviour
    {

        #region Read only

        private readonly string doorTag = "Door";

        private readonly string wallTag = "Wall";

        #endregion

        #region Serialized Fields

        [SerializeField] private float checkerRadius;

        [SerializeField] private List<RoomTracker> allRooms = new List<RoomTracker>();

        [SerializeField] private Queue<RoomTracker> roomsToCheck = new Queue<RoomTracker>();

        [SerializeField] private RoomTracker startingRoom;
        
        [SerializeField] private GameObject roomPrefab;

        [SerializeField] private LevelGenerationRulesData levelGenerationRulesData;

        [Range(1,10)]
        [SerializeField] private int levelOfDifficulty = 1;

        [SerializeField] private LayerMask roomCheckLayer;
        
        [SerializeField] private LayerMask doorCheckLayer;

        #endregion

        #region Private Fields

        [SerializeField]
        private RoomTracker _currentProcessRoom;

        private LevelGenerationRulesData.RoomByPercentage _cachedRoomByPercentage;

        private Transform _checkerTransform;

        private Vector3 _doorCheckerPosition;

        private bool _isGeneratingRooms;

        private bool _isGeneratingLevel;

        private float _calcPercentage;

        private int _currentLevel;

        #endregion
        
        #region Unity Events

        private void OnDrawGizmos()
        {
            if (_checkerTransform)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(_checkerTransform.position, checkerRadius);
            }
            
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(_doorCheckerPosition, checkerRadius);
            
        }

        #endregion

        #region Class Implementation

        [ContextMenu("Generate Level")]
        private void GenerateLevel()
        {
            if (!startingRoom)
            {
                Debug.LogError("Starting room doesn't contain reference");
                return;
            }

            if (allRooms.Count > 0)
            {
                allRooms.Clear();
            }

            if (roomsToCheck.Count > 0)
            {
                roomsToCheck.Clear();
            }
            
            allRooms.Add(startingRoom);
            roomsToCheck.Enqueue(startingRoom);

            _isGeneratingLevel = true;

            StartCoroutine(GenerateRooms());
        }

        private IEnumerator GenerateRooms()
        {
            while (_isGeneratingLevel)
            {
                var currentRoom = roomsToCheck.Dequeue();
                if (!currentRoom)
                {
                    yield break;
                }

                _currentLevel = currentRoom.level;
                _calcPercentage = _currentLevel / levelOfDifficulty;
                if (_currentLevel <= levelOfDifficulty)
                {
                    
                    //One door rooms can not generate new rooms
                    if (currentRoom.roomType == RoomType.ONE_DOOR)
                    {
                        yield return null;
                    }
                    
                    _currentProcessRoom = currentRoom;
                
                    //Generate rooms connected to current room
                    //Current room puts up doors
                    foreach (var doorChecker in currentRoom.doorCheckers)
                    {
                        _isGeneratingRooms = true;
                        _checkerTransform = doorChecker.roomChecker; 
                        GenerateConnectingRoom(doorChecker);
                        yield return new WaitUntil(() => !_isGeneratingRooms);
                    }
                
                }

                yield return new WaitForEndOfFrame();
            
                if (roomsToCheck.Count == 0)
                {
                    _isGeneratingLevel = false;
                    Debug.Log("Level generation finished");
                    yield break;
                }
                
                Debug.Log($"Room Generation Finished /// {roomsToCheck.Count} left");
            }

        }


        public void GenerateConnectingRoom(DoorChecker m_doorChecker)
        {
            Collider[] colliders = Physics.OverlapSphere(_checkerTransform.position, checkerRadius, roomCheckLayer);
            
            //If there is another room already in this direction
            if (colliders.Length > 0)
            {
                CheckDoorArea(m_doorChecker);
                _isGeneratingRooms = false;
                return;
            }
            
            SpawnNewRoom();
        }

        public void SpawnNewRoom()
        {
            var m_newRoom = Instantiate(roomPrefab, _checkerTransform.position, _checkerTransform.rotation);

            var m_newRoomTracker = m_newRoom.GetComponent<RoomTracker>();
            if (!m_newRoomTracker)
            {
                Debug.LogError("New Room Tracker doesn't exist");
                _isGeneratingRooms = false;
                return;
            }

            var m_randomRoomTypeByRule = GetRoomTypeByRule();
            var m_roomTypeByWeight = GetRoomTypeByWeight(m_randomRoomTypeByRule);

            m_newRoomTracker.roomType = m_roomTypeByWeight;
            m_newRoomTracker.level = _currentLevel + 1;

            if (m_newRoomTracker.roomType != RoomType.ONE_DOOR)
            {
                roomsToCheck.Enqueue(m_newRoomTracker);
            }

            allRooms.Add(m_newRoomTracker);

            ManageRoomDoors(m_newRoomTracker);
        }

        private void CheckDoorArea(DoorChecker m_doorChecker)
        {
            Collider[] colliders = Physics.OverlapSphere(m_doorChecker.doorCheckPosition, checkerRadius, doorCheckLayer);
            _doorCheckerPosition = m_doorChecker.doorCheckPosition;
            
            if (colliders.Length > 0)
            {
                for (int i = 0; i < colliders.Length; i++)
                {
                    if (colliders[i].CompareTag(doorTag))
                    {
                        Debug.Log("<color=orange>Obstruction found /// Door</color>");
                        m_doorChecker.AssignWallDoor(DoorType.DOORWAY);
                    }else if (colliders[i].CompareTag(wallTag))
                    {
                        Debug.Log("<color=yellow>Obstruction found /// Wall</color>");
                        m_doorChecker.AssignWallDoor(DoorType.WALL);
                    }
                }
            }
            else
            {
                Debug.Log("<color=cyan>No Obstruction found /// Door placed</color>");
                m_doorChecker.AssignWallDoor(DoorType.DOOR);
            }
        }

        private void ManageRoomDoors(RoomTracker m_roomTracker)
        {
            if (!m_roomTracker)
            {
                _isGeneratingRooms = false;
                return;
            }

            //Find connecting door
            var m_dirToCurrentRoom =  m_roomTracker.transform.position.FlattenVector3Y() - _currentProcessRoom.transform.position.FlattenVector3Y();

            DoorChecker m_connectedDoor = null;
            
            //Walls face in towards parent room
            foreach (var m_doorChecker in m_roomTracker.doorCheckers)
            {
                var dot = Vector3.Dot(m_doorChecker.transform.forward, m_dirToCurrentRoom);
                if (dot >= 0.9f)
                {
                    m_connectedDoor = m_doorChecker;
                    break;
                }
            }
            
            //remove door from door checkers
            if (m_connectedDoor)
            {
                CheckDoorArea(m_connectedDoor);
                m_roomTracker.doorCheckers.Remove(m_connectedDoor);
            }

            //If the room only has one door, the one door is the connecting door
            if (m_roomTracker.roomType == RoomType.ONE_DOOR || m_roomTracker.roomType == RoomType.FOUR_DOOR)
            {
                if (m_roomTracker.roomType == RoomType.ONE_DOOR)
                {
                    foreach (var doorChecker in m_roomTracker.doorCheckers)
                    {
                        doorChecker.AssignWallDoor(DoorType.WALL);
                    }
                }
                _isGeneratingRooms = false;
                return;
            }

            var m_roomTypeInt = (int)m_roomTracker.roomType;

            //add random doors equal to room type
            for (int i = 0; i < m_roomTypeInt; i++)
            {
                int m_randomInt = Random.Range(0, m_roomTracker.doorCheckers.Count);
                var m_selectedDoor = m_roomTracker.doorCheckers[m_randomInt];
                m_selectedDoor.AssignWallDoor(DoorType.WALL);
                m_roomTracker.doorCheckers.Remove(m_selectedDoor);
            }
            
            _isGeneratingRooms = false;
            
        }

        public LevelGenerationRulesData.RoomByPercentage GetRoomTypeByRule()
        {
            //default value
            LevelGenerationRulesData.RoomByPercentage m_roomByPercentages = levelGenerationRulesData.defaultRoomByPercentage;

            if (_cachedRoomByPercentage != null && _calcPercentage <= _cachedRoomByPercentage.percentage)
            {
                return _cachedRoomByPercentage;
            }

            for (int i = 0; i < levelGenerationRulesData.roomByPercentages.Count; i++)
            {
                if (i == 0)
                {
                    //Less than lowest percentage
                    if (_calcPercentage <= levelGenerationRulesData.roomByPercentages[i].percentage)
                    {
                        m_roomByPercentages = levelGenerationRulesData.roomByPercentages[i];
                        break;
                    }
                }else if(i == levelGenerationRulesData.roomByPercentages.Count - 1)
                {
                    //higher than second highest percentage
                    if (_calcPercentage >= levelGenerationRulesData.roomByPercentages[i].percentage ||
                        _calcPercentage >= levelGenerationRulesData.roomByPercentages[i-1].percentage)
                    {
                        m_roomByPercentages = levelGenerationRulesData.roomByPercentages[i];
                        break;
                    }
                }
                else
                {
                    //falls in between two percentages = higher percentage rule
                    if (_calcPercentage >= levelGenerationRulesData.roomByPercentages[i-1].percentage &&
                        _calcPercentage < levelGenerationRulesData.roomByPercentages[i].percentage)
                    {
                        m_roomByPercentages = levelGenerationRulesData.roomByPercentages[i];
                        break;
                    }
                }
            }

            _cachedRoomByPercentage = m_roomByPercentages;
            return m_roomByPercentages;
        }

        public RoomType GetRoomTypeByWeight(LevelGenerationRulesData.RoomByPercentage m_roomByPercentage)
        {
            //default value
            RoomType endValue = RoomType.FOUR_DOOR;

            var listOfPossibleRooms = m_roomByPercentage;
            
            //get random weight value from wave weight
            var totalWeight = 0;
            foreach (var roomByWeight in listOfPossibleRooms.roomsByWeights)
            {
                totalWeight += roomByWeight.weight;
            }
            //+1 because max value is exclusive
            var randomValue = Random.Range(1, totalWeight + 1);

            var currentWeight = 0;
            foreach (var roomByWeight in listOfPossibleRooms.roomsByWeights)
            {
                currentWeight += roomByWeight.weight;
                if (randomValue <= currentWeight)
                {
                    endValue = roomByWeight.roomType;
                    break;
                }
            }

            return endValue;
        }

        #endregion

    }
}