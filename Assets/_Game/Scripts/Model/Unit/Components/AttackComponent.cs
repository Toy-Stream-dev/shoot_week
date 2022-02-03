using _Game.Scripts.Enums;
using _Game.Scripts.Model.Items;
using _Game.Scripts.Model.Numbers;
using _Game.Scripts.View;
using GeneralTools.Model;
using UnityEngine;

namespace _Game.Scripts.Model.Unit.Components
{
    public class AttackComponent : BaseComponent<UnitModel>
    {
        public enum AttackState 
        {
            None,
            Wait,
            Attack,
            Reload,
            Stop,
        }

        private AttackState _state;

        //private UnitView _view;
        private double _delay;
        private double _cd;
        private bool _hasTarget;

        private const float _rotationSpeed = 5f;
        public AttackState State => _state;
        
        public AttackComponent(UnitModel model) : base(model)
        {
            //_view = model.View;
        }

        public override void Update(float deltaTime)
        {
            if(Model.State == UnitModel.UnitState.Death) return;
            if (Model.Target == null || !Model.HasTarget())
            {
                SetTargetAvailability(false);
                return;
            }
            SetTargetAvailability(true);
            Model.View.WeaponIK.SetTargetPosition(Model.Target.View.BulletPoint.position);
            UpdateRotation(deltaTime);

            switch (_state)
            {
                case AttackState.None:
                case AttackState.Attack:
                    PlayAttack();
                    break;
                
                case AttackState.Wait:
                    _cd -= deltaTime;
                    if (_cd <= 0)
                    {
                        _state = AttackState.Attack;
                    }
                    break;
            }
            base.Update(deltaTime);
        }

        private void UpdateRotation(float deltaTime)
        {
            if(Model.Type != UnitType.Player) return;
            var newDir = Vector3.RotateTowards(Model.View.transform.forward, 
                Model.Target.View.transform.position - Model.View.transform.position, 
                _rotationSpeed * deltaTime, 
                0.0F);

            var lookRotation = Quaternion.LookRotation(newDir); 
            lookRotation.x = 0.0f;
            lookRotation.z = 0.0f;
            
            Model.Rotate(lookRotation); 
        }

        private void SetTargetAvailability(bool value)
        {
            if(_hasTarget == value) return;
            _hasTarget = value;
            Model.View.WeaponIK.HasTarget = _hasTarget;
        }

        private void OnTargetDie()
        {
            SetTargetAvailability(false);
        }

        public void InitWeaponConfig()
        {
            _delay = Model.Weapon.GetParam(GameParamType.AttackSpeed).Value;
            _cd = _delay;
        }

        private void PlayAttack()
        {
            switch (Model.Target.State)
            {
                case UnitModel.UnitState.Death:
                    return;
                case UnitModel.UnitState.Idle:
                    if (Model.Type != UnitType.Player)
                    {
                        Model.GoToAttackPlayer(true, true);   
                    }
                    break;
            }
            if(Model.Type == UnitType.Player && !Model.Target.View.CheckVisible()) return;
            if (!Model.View.WeaponIK.CanShot() || !CheckAmmo()) return;
            Model.View.PlayAnimation(UnitAnimationType.Attack);
            _cd = _delay;
            _state = AttackState.Wait;   
        }

        public bool CheckAmmo()
        {
            if (Model.Weapon.Class == ItemClass.Weapon)
            {
                if (Model.Weapon.Ammunition.Value > 0)
                {
                    return true;
                }
                StartReload();
                Model.View.PlayAnimation(UnitAnimationType.Reload);

                return false;
            }

            return true;
        }

        public void StartReload()
        {
            _state = AttackState.Reload;
        }

        public void EndReload()
        {
            _state = AttackState.Attack;
        }

        public void StartAttack()
        {
            if (_state == AttackState.Stop)
            {
                _state = AttackState.Attack;   
            }
        }

        public void StopAttack()
        {
            //_state = AttackState.Stop;
        }
    }
}