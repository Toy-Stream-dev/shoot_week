using System;
using GeneralTools;
using UnityEngine;

namespace _Game.Scripts.View.Unit
{
    [System.Serializable]
    public class HumanBone
    {
        public HumanBodyBones Bone;
    }
    
    public class WeaponIK : BaseBehaviour
    {
        [SerializeField] private Transform _targetTransform;
        [SerializeField] private Transform _aimTransform;
        [Range(1, 10)] [SerializeField] private int _iteration = 10;
        [Range(0, 1)] [SerializeField] private float _weight = 1f;
        [SerializeField] private float _angleLimit = 90f;
        [SerializeField] private Transform[] _humanBones;
        private float _distanceLimit = 0.5f;

        public bool HasTarget { get; set; }

        public void SetTargetPosition(Vector3 pos)
        {
            _targetTransform.position = pos;
        }

        public void SetWeight(float weight)
        {
            _weight = weight;
        }
        
        private void LateUpdate()
        {
            if(!HasTarget) return;
            var targetPosition = GetTargetPosition();
            for (var i = 0; i < _iteration; i++)
            {
                foreach (var bone in _humanBones)
                {
                    AimAtTarget(bone, targetPosition, _weight);
                }
            }
        }

        private Vector3 GetTargetPosition()
        {
            var targetDirection = _targetTransform.position - _aimTransform.position;
            var aimDirection = _aimTransform.forward;
            var blendOut = 0.0f;

            var targetAngle = Vector3.Angle(targetDirection, aimDirection);
            if (targetAngle > _angleLimit)
            {
                blendOut += (targetAngle - _angleLimit) / 50f;
            }

            var targetDistance = targetDirection.magnitude;
            if (targetDistance < _distanceLimit)
            {
                blendOut += _distanceLimit - targetDistance;
            }

            var direction = Vector3.Slerp(targetDirection, aimDirection, blendOut);
            return _aimTransform.position + direction;
        }

        private void AimAtTarget(Transform bone, Vector3 targetPosition, float weight)
        {
            var aimDirection = _aimTransform.forward;
            var targetDirection = targetPosition - _aimTransform.position;
            var aimTowards = Quaternion.FromToRotation(aimDirection, targetDirection);
            var blendedRotation = Quaternion.Slerp(Quaternion.identity, aimTowards, weight);
            bone.rotation = blendedRotation * bone.rotation;
        }

        public bool CanShot()
        {
            var position = _aimTransform.position;
            var ray = new Ray(position, _targetTransform.position - position);
            //Debug.DrawRay(position, (_targetTransform.position - position) * 50);
            if (Physics.Raycast(ray, out RaycastHit hit, 60, ~(1 << 7)))
            {
                var o = hit.transform.gameObject;
                return o.layer == 8 || o.layer == 10;
            }

            return false;
        }
    }
}