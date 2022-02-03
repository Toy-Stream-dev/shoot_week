using System;
using System.Collections.Generic;
using _Game.Scripts.Enums;
using _Game.Scripts.Model.Items;
using _Game.Scripts.Model.Numbers;
using _Game.Scripts.Model.Unit;
using _Game.Scripts.View.Unit;
using GeneralTools;
using GeneralTools.Tools;
using UnityEngine;

namespace _Game.Scripts.View
{
    public enum ProjectileType
    {
        None,
        Enemy,
        Player,
    } 
    public class ProjectileView : BaseBehaviour
    {
        public Action<ProjectileView> OnDestroy;
        //public Action<ProjectileView, bool, float> OnEndMove;
        [SerializeField] private UnitType _projectileType;
        [SerializeField] private List<ParticleSystem> _bodies;
        [SerializeField] private ParticleSystem _bulletImpact;
        private GameProgress _pushTimer;
        private ProjectileState _state;
        private BattleSettings.ProjectileConfig _config;
        private Vector3 _direction;
        private float _timer;
        private float _speed;
        private float _damage;
        private UnitModel _owner;

        public UnitType ProjectileType => _projectileType;

        public void Init(BattleSettings.ProjectileConfig config, Vector3 position, Quaternion rotation, float damage, UnitModel owner)
        {
            _owner = owner;
            _damage = damage;
            foreach (var body in _bodies)
            {
                body.Activate().Play();
            }
            _config = config;
            _timer = config.Duration;
            _speed = config.Speed;
            _pushTimer = new GameProgress(GameParamType.Timer, _config.PushDelay, false);
            _pushTimer.CompletedEvent += Push;

            transform.position = position;
            transform.rotation = rotation;
            _direction = transform.position + transform.forward * 20;
            transform.localScale = config.Scale;
            SetState(ProjectileState.Move);
        }

        public override void UpdateMe(float deltaTime)
        {
            switch (_state)
            {
                case ProjectileState.Move:

                    var pos = Vector3.MoveTowards(transform.position, _direction, _speed * deltaTime);
                    transform.position = pos;
                    transform.LookAt(_direction);
                    if ((_direction - transform.position).sqrMagnitude > 0.01f) return;
                    SetState(ProjectileState.Stopped);
                    _pushTimer.Pause();
                    Push();
                    break;
                case ProjectileState.Stopped:
                    _pushTimer.Change(deltaTime);
                    break;
            }

            base.UpdateMe(deltaTime);
        }

        private void OnTriggerEnter(Collider other)
        {
            if(_state == ProjectileState.Stopped) return;
            switch (other.gameObject.layer)
            {
                case 8:
                    var listener = other.GetComponent<CollisionListener>();
                    if (listener != null)
                    {
                        if(listener.UnitModel.Type == _owner.Type) return;
                        listener.UnitModel.Hit(_damage);
                        SetState(ProjectileState.Stopped);
                        _pushTimer.Pause();
                        Push();
                    }
                    break;
                case 12:
                    _bulletImpact.Play();
                    SetState(ProjectileState.Stopped);
                    break;
            }
        }

        private void Push()
        {
            _owner = null;
            _pushTimer.CompletedEvent -= Push;
            OnDestroy?.Invoke(this);
        }

        private void SetState(ProjectileState state)
        {
            _state = state;
            switch (_state)
            {
                case ProjectileState.Stopped:
                    foreach (var body in _bodies)
                    {
                        body.Deactivate();
                    }
                    break;
            }
        }
    }
}