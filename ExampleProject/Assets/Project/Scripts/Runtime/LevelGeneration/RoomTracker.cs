using System.Collections.Generic;
using UnityEngine;

namespace Project.Scripts.Runtime.LevelGeneration
{
    [DisallowMultipleComponent]
    public class RoomTracker: MonoBehaviour
    {

        #region Public Fields

        public int level;

        public RoomType roomType;

        public List<DoorChecker> modifiableDoorCheckers = new List<DoorChecker>();

        #endregion

        #region Serialized Fields

        [SerializeField]
        private List<DoorChecker> doorCheckers = new List<DoorChecker>();

        #endregion
        
        #region Class Implementation

        public void ResetRoom()
        {
            level = 0;
            roomType = RoomType.FOUR_DOOR;
            doorCheckers.ForEach(dc =>
            {
                dc.ResetWalls();
                if (!modifiableDoorCheckers.Contains(dc))
                {
                    modifiableDoorCheckers.Add(dc);
                }
            });
        }

        #endregion

    }
}