using System.Collections.Generic;
using UnityEngine;

namespace Project.Scripts.Runtime.LevelGeneration
{
    [DisallowMultipleComponent]
    public class RoomTracker: MonoBehaviour
    {

        #region Public Fields

        public List<RoomChecker> _roomCheckers = new List<RoomChecker>();

        public List<DoorChecker> _doorCheckers = new List<DoorChecker>();

        #endregion

    }
}