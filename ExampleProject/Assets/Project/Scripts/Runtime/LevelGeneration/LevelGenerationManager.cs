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

        [SerializeField] private Queue<RoomChecker> _roomsToCheck = new Queue<RoomChecker>();

        #endregion

        #region Private Fields

        private Vector3 checkerPosition;

        #endregion
        
        #region Unity Events
        
        

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(checkerPosition, checkerRadius);
        }

        #endregion

        #region Class Implementation

        [ContextMenu("Generate Level")]
        private void GenerateLevel()
        {
            
        }

        #endregion

    }
}