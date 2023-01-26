using System;
using System.Collections.Generic;
using Project.Scripts.Runtime.LevelGeneration;
using UnityEngine;

namespace Project.Scripts.Data
{
    [CreateAssetMenu(menuName = "Custom Data/Level Generation Settings")]
    public class LevelGenerationRulesData : ScriptableObject
    {

        #region Nested Classes

        [Serializable]
        public class RoomByWeight
        {
            public int weight;
            public RoomType roomType;
        }

        [Serializable]
        public class RoomByPercentage
        {
            [Range(0,1f)]
            public float percentage;
            public List<RoomByWeight> roomsByWeights = new List<RoomByWeight>();
        }

        #endregion

        #region Public Fields

        public List<RoomByPercentage> roomByPercentages = new List<RoomByPercentage>();

        public RoomByPercentage defaultRoomByPercentage;
        
        #endregion
    }
}