using System;
using GeneralTools;
using UnityEngine;

namespace _Game.Scripts.View.Unit
{
    public class MeshSocket : BaseBehaviour
    {
        public SocketId SocketId;
        [SerializeField] private Transform _attachPoint;

        private void Awake()
        {
            _attachPoint = transform.GetChild(0); 
        }

        public void Attach(Transform objTransform)
        {
            objTransform.SetParent(_attachPoint, false);
        }
    }
}