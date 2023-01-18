using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Project.Scripts.Runtime.LevelGeneration
{
    [DisallowMultipleComponent]
    public class LevelGenerationManager: MonoBehaviour
    {

        #region Serialized Fields

        [SerializeField] private float checkerRadius;

        [SerializeField] private List<RoomTracker> _allRooms = new List<RoomTracker>();

        [SerializeField] private Queue<RoomTracker> _roomsToCheck = new Queue<RoomTracker>();

        [SerializeField] private RoomTracker _startingRoom;

        #endregion

        #region Private Fields

        private Vector3 _checkerPosition;

        private bool _isGeneratingRooms;

        private bool _isGeneratingLevel;

        #endregion
        
        #region Unity Events

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(_checkerPosition, checkerRadius);
        }

        #endregion

        #region Class Implementation

        [ContextMenu("Generate Level")]
        private void GenerateLevel()
        {
            if (!_startingRoom)
            {
                Debug.LogError("Starting room doesn't contain reference");
                return;
            }

            if (_allRooms.Count > 0)
            {
                _allRooms.Clear();
            }

            if (_roomsToCheck.Count > 0)
            {
                _roomsToCheck.Clear();
            }
            
            _allRooms.Add(_startingRoom);
            _roomsToCheck.Enqueue(_startingRoom);
            
        }

        private IEnumerator GenerateRoom()
        {
            _isGeneratingRooms = true;
            yield return new WaitForEndOfFrame();
            _isGeneratingRooms = false;
        }


        private void CheckRoomPosition()
        {
            
        }

        private void CheckDoorPosition()
        {
            
        }

        #endregion

    }
}