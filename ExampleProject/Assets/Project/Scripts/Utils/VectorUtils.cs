using UnityEngine;

namespace Project.Scripts.Utils
{
    public static class VectorUtils
    {

        #region Class Implementation

        public static Vector3 FlattenVector3Y(this Vector3 vector)
        {
            vector.y = 0;
            return vector;
        }

        #endregion
        
    }
}