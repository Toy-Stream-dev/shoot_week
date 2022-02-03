using System;
using System.Collections.Generic;
using System.Threading;
using GeneralTools;
using UnityEngine;

namespace _Game.Scripts.View.Unit
{
    public enum SocketId
    {
        Spine,
        RightHand,
    }
    public class MeshSockets : BaseBehaviour
    {
        private Dictionary<SocketId, MeshSocket> _socketMap = new Dictionary<SocketId, MeshSocket>();

        private void Awake()
        {
            var sockets = GetComponentsInChildren<MeshSocket>();
            foreach (var socket in sockets)
            {
                _socketMap[socket.SocketId] = socket;
            }
        }

        public void Attach(Transform objTransform, SocketId socketId)
        {
            _socketMap[socketId].Attach(objTransform); 
        }
    }
}