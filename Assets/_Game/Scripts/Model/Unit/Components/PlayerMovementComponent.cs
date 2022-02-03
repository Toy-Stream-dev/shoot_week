using _Game.Scripts.Enums;
using GeneralTools.Model;
using UnityEngine;

namespace _Game.Scripts.Model.Unit.Components
{
    public class PlayerMovementComponent : BaseComponent<UnitModel>
    {
        private readonly float _speed;
        private const float _rotationSpeed = 5f;
        private Vector3 _deltaPosition;

        public PlayerMovementComponent(UnitModel model) : base(model)
        {
            _speed = model.GetParam(GameParamType.Speed).Value;
        }

        public void Move(Vector2 direction, float deltaTime)
        {
            //_deltaPosition = direction.normalized * (_speed * deltaTime);
            if (direction == Vector2.zero)
            {
                Model.Move(_deltaPosition);
                Model.SetState(UnitModel.UnitState.Idle);
                Model.SetTarget();
                return;
            }

            Model.Move(direction);
            Model.SetTarget();

            //if (Model.HasTarget()) return;
            
            // var lookRotation = Quaternion.LookRotation(direction);
            // lookRotation.x = 0.0f;
            // lookRotation.z = 0.0f;
            //
            // var rotation = Quaternion.Slerp(Model.Rotation, lookRotation, deltaTime * _rotationSpeed);
            // Model.Rotate(rotation);
        }

        public void Stop()
        {
            Model.SetState(UnitModel.UnitState.Idle);
        }
    }
}