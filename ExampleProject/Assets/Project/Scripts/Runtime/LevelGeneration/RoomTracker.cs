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
        
        public List<DoorChecker> doorCheckers = new List<DoorChecker>();
        
        #endregion

    }
}