using System;
using GeneralTools;
using UnityEngine;

namespace _Game.Scripts.View.Unit
{
    public class DrawLineDebug : BaseBehaviour
    {
        private void OnDrawGizmos()
        {
            //Debug.DrawLine(transform.position, transform.position + transform.forward * 50, Color.red);
        }
    }
}