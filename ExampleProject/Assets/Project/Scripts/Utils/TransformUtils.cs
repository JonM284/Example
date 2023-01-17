﻿using UnityEngine;

namespace Project.Scripts.Utils
{
    public static class TransformUtils
    {
        
        #region Read-Only

        private static readonly string poolTag = "Pool";

        #endregion
        
        #region Class Implementation

        public static Transform CreatePool(Transform parentObject)
        {
            if (parentObject == null)
            {
                return null;
            }

            var t = new GameObject(parentObject.name + poolTag).transform;
            t.ResetTransform(parentObject);
            t.gameObject.SetActive(false);
            return t;
        }

        public static void ResetTransform(this Transform newTransform, Transform parentTransform = null)
        {
            if (parentTransform != null)
            {
                newTransform.parent = parentTransform;
                newTransform.position = parentTransform.position;
                return;
            }
            
            newTransform.position = Vector3.zero;
            newTransform.rotation = Quaternion.identity;
        }

        #endregion
        
        
        
        
    }
}