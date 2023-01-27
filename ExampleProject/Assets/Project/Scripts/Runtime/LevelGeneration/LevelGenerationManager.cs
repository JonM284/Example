using System.Collections;
using System.Collections.Generic;
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

        private readonly string m_doorTag = "Door";

        private readonly string m_wallTag = "Wall";

        #endregion

        #region Serialized Fields

        [SerializeField] private float checkerRadius;

        [SerializeField] private List<RoomTracker> allRooms = new List<RoomTracker>();
        
        [SerializeField] private RoomTracker startingRoom;
        
        [SerializeField] private GameObject roomPrefab;

        [SerializeField] private LevelGenerationRulesData levelGenerationRulesData;

        [Range(1,10)]
        [SerializeField] private int levelOfDifficulty = 1;

        [SerializeField] private LayerMask roomCheckLayer;
        
        [SerializeField] private LayerMask doorCheckLayer;

        #endregion

        #region Private Fields

        private RoomTracker m_currentProcessRoom;

        private List<RoomTracker> m_cachedRoomTrackers = new List<RoomTracker>();

        private Queue<RoomTracker> m_roomsToCheck = new Queue<RoomTracker>();

        private LevelGenerationRulesData.RoomByPercentage m_cachedRoomByPercentage;

        private Transform m_checkerTransform;

        private Vector3 m_doorCheckerPosition;

        private bool m_isGeneratingRooms;

        private bool m_isGeneratingLevel;

        private float m_calcPercentage;

        private int m_currentLevel;

        private Transform m_inactiveRoomPool;

        private Transform m_activeRoomPool;

        #endregion

        #region Accessors

        public Transform inactiveRoomPool =>
            CommonUtils.GetRequiredComponent(ref m_inactiveRoomPool, ()=>
            {
                var poolTransform = TransformUtils.CreatePool(this.transform, false);
                return poolTransform;
            });
        
        public Transform activeRoomPool => CommonUtils.GetRequiredComponent(ref m_activeRoomPool, ()=>
        {
            var poolTransform = TransformUtils.CreatePool(this.transform, true);
            poolTransform.RenameTransform("LevelRoomPool");
            return poolTransform;
        });

        #endregion
        
        #region Unity Events

        private void OnDrawGizmos()
        {
            if (m_checkerTransform)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(m_checkerTransform.position, checkerRadius);
            }
            
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(m_doorCheckerPosition, checkerRadius);
            
        }

        #endregion

        #region Class Implementation

        [ContextMenu("Generate Level")]
        private void GenerateLevel()
        {
            if (!startingRoom && !roomPrefab)
            {
                Debug.LogError("Starting room doesn't contain reference");
                return;
            }

            if (!startingRoom && roomPrefab)
            {
                var _newStartingRoom = roomPrefab.Clone(activeRoomPool);
                var _newStartingRoomTracker = _newStartingRoom.GetComponent<RoomTracker>();
                startingRoom = _newStartingRoomTracker;
            }

            if (allRooms.Count > 0)
            {
                allRooms.ForEach(rt =>
                {
                    rt.ResetRoom();
                    if (rt != startingRoom)
                    {
                        m_cachedRoomTrackers.Add(rt);
                        rt.transform.ResetTransform(inactiveRoomPool);   
                    }
                });
                allRooms.Clear();
            }

            if (m_roomsToCheck.Count > 0)
            {
                m_roomsToCheck.Clear();
            }

            m_cachedRoomByPercentage = null;
            
            allRooms.Add(startingRoom);
            m_roomsToCheck.Enqueue(startingRoom);

            m_isGeneratingLevel = true;

            StartCoroutine(GenerateRooms());
        }

        private IEnumerator GenerateRooms()
        {
            while (m_isGeneratingLevel)
            {
                var _currentRoom = m_roomsToCheck.Dequeue();
                if (!_currentRoom)
                {
                    yield break;
                }

                m_currentLevel = _currentRoom.level;
                m_calcPercentage = m_currentLevel / levelOfDifficulty;
                if (m_currentLevel <= levelOfDifficulty)
                {
                    
                    //One door rooms can not generate new rooms
                    if (_currentRoom.roomType == RoomType.ONE_DOOR)
                    {
                        yield return null;
                    }
                    
                    m_currentProcessRoom = _currentRoom;
                
                    //Generate rooms connected to current room
                    //Current room puts up doors
                    foreach (var doorChecker in _currentRoom.modifiableDoorCheckers)
                    {
                        m_isGeneratingRooms = true;
                        m_checkerTransform = doorChecker.roomChecker; 
                        GenerateConnectingRoom(doorChecker);
                        yield return new WaitUntil(() => !m_isGeneratingRooms);
                    }
                
                }

                yield return null;
            
                if (m_roomsToCheck.Count == 0)
                {
                    m_isGeneratingLevel = false;
                    Debug.Log("Level generation finished");
                    yield break;
                }
                
            }

        }
        
        public void GenerateConnectingRoom(DoorChecker _doorChecker)
        {
            Collider[] colliders = Physics.OverlapSphere(m_checkerTransform.position, checkerRadius, roomCheckLayer);
            
            //If there is another room already in this direction
            if (colliders.Length > 0)
            {
                CheckDoorArea(_doorChecker);
                m_isGeneratingRooms = false;
                return;
            }
            
            SpawnNewRoom();
        }

        public void SpawnNewRoom()
        {
            var _newRoom = m_cachedRoomTrackers.Count > 0
                ? m_cachedRoomTrackers[0].gameObject
                : roomPrefab.Clone(activeRoomPool);

            if (_newRoom.transform.parent != activeRoomPool)
            {
                _newRoom.transform.ResetTransform(activeRoomPool);
            }
            
            _newRoom.transform.ResetPRS(m_checkerTransform);
            
            var _newRoomTracker = _newRoom.GetComponent<RoomTracker>();
            if (!_newRoomTracker)
            {
                Debug.LogError("New Room Tracker doesn't exist");
                m_isGeneratingRooms = false;
                return;
            }

            if (m_cachedRoomTrackers.Count > 0 && m_cachedRoomTrackers.Contains(_newRoomTracker))
            {
                m_cachedRoomTrackers.Remove(_newRoomTracker);
            }
            
            var m_randomRoomTypeByRule = GetRoomTypeByRule();
            var m_roomTypeByWeight = GetRoomTypeByWeight(m_randomRoomTypeByRule);

            _newRoomTracker.roomType = m_roomTypeByWeight;
            _newRoomTracker.level = m_currentLevel + 1;

            //If the room is single door, it does not have to be processed
            if (_newRoomTracker.roomType != RoomType.ONE_DOOR)
            {
                m_roomsToCheck.Enqueue(_newRoomTracker);
            }

            allRooms.Add(_newRoomTracker);

            ManageRoomDoors(_newRoomTracker);
        }

        /// <summary>
        /// Place door type depending on rule
        /// Found: DOOR => place: DOORWAY
        /// Found: WALL => place: WALL
        /// Found: Nothing => place: DOOR
        /// </summary>
        /// <param name="m_doorChecker">Current door to check</param>
        private void CheckDoorArea(DoorChecker _doorChecker)
        {
            Collider[] _foundColliders = Physics.OverlapSphere(_doorChecker.doorCheckPosition, checkerRadius, doorCheckLayer);
            m_doorCheckerPosition = _doorChecker.doorCheckPosition;
            
            if (_foundColliders.Length > 0)
            {
                foreach (var foundCollider in _foundColliders)
                {
                    if (foundCollider.CompareTag(m_doorTag))
                    {
                        _doorChecker.AssignWallDoor(DoorType.DOORWAY);
                    }else if (foundCollider.CompareTag(m_wallTag))
                    {
                        _doorChecker.AssignWallDoor(DoorType.WALL);
                    }
                }
            }
            else
            {
                _doorChecker.AssignWallDoor(DoorType.DOOR);
            }
        }

        private void ManageRoomDoors(RoomTracker _roomTracker)
        {
            if (!_roomTracker)
            {
                m_isGeneratingRooms = false;
                return;
            }

            //Find connecting door between current room and created room
            var _dirToCurrentRoom =  _roomTracker.transform.position.FlattenVector3Y() - m_currentProcessRoom.transform.position.FlattenVector3Y();

            DoorChecker _connectedDoor = null;
            
            //Walls face in towards parent room
            foreach (var currentRoomDoorChecker in _roomTracker.modifiableDoorCheckers)
            {
                var _dot = Vector3.Dot(currentRoomDoorChecker.transform.forward, _dirToCurrentRoom);
                if (_dot >= 0.9f)
                {
                    _connectedDoor = currentRoomDoorChecker;
                    break;
                }
            }
            
            //remove door from door checkers
            if (_connectedDoor)
            {
                CheckDoorArea(_connectedDoor);
                _roomTracker.modifiableDoorCheckers.Remove(_connectedDoor);
            }

            //If the room only has one door, the one door is the connecting door
            if (_roomTracker.roomType == RoomType.ONE_DOOR || _roomTracker.roomType == RoomType.FOUR_DOOR)
            {
                
                //If the room has only one door, this door is the connecting door -> all other sides become walls
                if (_roomTracker.roomType == RoomType.ONE_DOOR)
                {
                    _roomTracker.modifiableDoorCheckers.ForEach(dc => dc.AssignWallDoor(DoorType.WALL));
                }
                
                m_isGeneratingRooms = false;
                return;
            }

            //Int from enum type will determine how many doors to remove
            var _roomTypeInt = (int)_roomTracker.roomType;

            //Add random walls depending on room type
            for (int i = 0; i < _roomTypeInt; i++)
            {
                int _randomInt = Random.Range(0, _roomTracker.modifiableDoorCheckers.Count);
                var _selectedDoor = _roomTracker.modifiableDoorCheckers[_randomInt];
                _selectedDoor.AssignWallDoor(DoorType.WALL);
                _roomTracker.modifiableDoorCheckers.Remove(_selectedDoor);
            }
            
            m_isGeneratingRooms = false;
            
        }

        public LevelGenerationRulesData.RoomByPercentage GetRoomTypeByRule()
        {
            //default value
            LevelGenerationRulesData.RoomByPercentage _roomByPercentages = levelGenerationRulesData.defaultRoomByPercentage;

            if (m_cachedRoomByPercentage != null && m_calcPercentage <= m_cachedRoomByPercentage.percentage)
            {
                return m_cachedRoomByPercentage;
            }

            for (int i = 0; i < levelGenerationRulesData.roomByPercentages.Count; i++)
            {
                if (i == 0)
                {
                    //Less than lowest percentage
                    if (m_calcPercentage <= levelGenerationRulesData.roomByPercentages[i].percentage)
                    {
                        _roomByPercentages = levelGenerationRulesData.roomByPercentages[i];
                        break;
                    }
                }else if(i == levelGenerationRulesData.roomByPercentages.Count - 1)
                {
                    //higher than second highest percentage
                    if (m_calcPercentage >= levelGenerationRulesData.roomByPercentages[i].percentage ||
                        m_calcPercentage >= levelGenerationRulesData.roomByPercentages[i-1].percentage)
                    {
                        _roomByPercentages = levelGenerationRulesData.roomByPercentages[i];
                        break;
                    }
                }
                else
                {
                    //falls in between two percentages = higher percentage rule
                    if (m_calcPercentage >= levelGenerationRulesData.roomByPercentages[i-1].percentage &&
                        m_calcPercentage < levelGenerationRulesData.roomByPercentages[i].percentage)
                    {
                        _roomByPercentages = levelGenerationRulesData.roomByPercentages[i];
                        break;
                    }
                }
            }

            m_cachedRoomByPercentage = _roomByPercentages;
            return _roomByPercentages;
        }

        public RoomType GetRoomTypeByWeight(LevelGenerationRulesData.RoomByPercentage _roomByPercentage)
        {
            //default value
            RoomType _endValue = RoomType.FOUR_DOOR;

            var _listOfPossibleRooms = _roomByPercentage;
            
            //get random weight value from room weight
            var _totalWeight = 0;
            foreach (var roomByWeight in _listOfPossibleRooms.roomsByWeights)
            {
                _totalWeight += roomByWeight.weight;
            }
            
            //+1 because max value is exclusive
            var _randomValue = Random.Range(1, _totalWeight + 1);

            var _currentWeight = 0;
            foreach (var roomByWeight in _listOfPossibleRooms.roomsByWeights)
            {
                _currentWeight += roomByWeight.weight;
                if (_randomValue <= _currentWeight)
                {
                    _endValue = roomByWeight.roomType;
                    break;
                }
            }

            return _endValue;
        }

        #endregion

    }
}