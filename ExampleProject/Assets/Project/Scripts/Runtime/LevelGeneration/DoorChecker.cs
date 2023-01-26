using System;
using System.Collections.Generic;
using UnityEngine;

namespace Project.Scripts.Runtime.LevelGeneration
{
    [DisallowMultipleComponent]
    public class DoorChecker: MonoBehaviour
    {

        #region Nested Classes

        [Serializable]
        class WallObstructionTypes
        {
            public DoorType doorType;
            public GameObject associatedObject;
        }

        #endregion

        #region Serialized Fields

        [SerializeField]
        private List<WallObstructionTypes> _wallObstructionTypesList = new List<WallObstructionTypes>();

        [SerializeField] private Transform _doorCheckPosition;
        
        [SerializeField] private Transform _associatedRoomCheckTransform;
        
        #endregion

        #region Accessors

        public Vector3 doorCheckPosition => _doorCheckPosition.position;

        public Transform roomChecker => _associatedRoomCheckTransform;

        #endregion

        #region Class Implementation

        //Turn on child object
        public void AssignWallDoor(DoorType m_doorType)
        {
            if (_wallObstructionTypesList.Count == 0)
            {
                Debug.LogError("No walls assigned");
                return;
            }
            
            _wallObstructionTypesList.ForEach(wot => wot.associatedObject.SetActive(wot.doorType == m_doorType));
        }

        public void ResetWalls()
        {
            _wallObstructionTypesList.ForEach(wot => wot.associatedObject.SetActive(false));
        }

        #endregion

    }
}