using System;
using UnityEngine;

namespace Project.Scripts.Runtime.LevelGeneration
{
    [RequireComponent(typeof(BoxCollider))]
    [DisallowMultipleComponent]
    public class DoorChecker: MonoBehaviour
    {

        #region Serialized Fields

        [SerializeField]
        private BoxCollider _boxCollider;

        #endregion

        #region Private Fields

        private bool _isObstructed;

        private string _obstructionObjectTag;

        #endregion

        #region Accessors

        public bool isObstructed => _isObstructed;

        public string obstructionObjectTag => _obstructionObjectTag;

        #endregion

        #region Unity Events

        private void OnTriggerEnter(Collider other)
        {
            _isObstructed = true;
            _obstructionObjectTag = other.gameObject.tag;
        }

        #endregion

        #region Class Implementation

        public void CheckDoor()
        {
            _boxCollider.enabled = true;
        }

        #endregion

    }
}