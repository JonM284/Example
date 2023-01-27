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
        private List<WallObstructionTypes> wallObstructionTypesList = new List<WallObstructionTypes>();

        [SerializeField] private Transform doorCheckTransform;
        
        [SerializeField] private Transform associatedRoomCheckTransform;
        
        #endregion

        #region Accessors

        public Vector3 doorCheckPosition => doorCheckTransform.position;

        public Transform roomChecker => associatedRoomCheckTransform;

        #endregion

        #region Class Implementation

        //Turn on child object
        public void AssignWallDoor(DoorType _doorType)
        {
            if (wallObstructionTypesList.Count == 0)
            {
                Debug.LogError("No walls assigned");
                return;
            }
            
            wallObstructionTypesList.ForEach(wot => wot.associatedObject.SetActive(wot.doorType == _doorType));
        }

        public void ResetWalls()
        {
            wallObstructionTypesList.ForEach(wot => wot.associatedObject.SetActive(false));
        }

        #endregion

    }
}