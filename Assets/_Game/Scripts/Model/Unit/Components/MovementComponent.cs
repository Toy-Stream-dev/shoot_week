using System.Collections.Generic;
using _Game.Scripts.Enums;
using _Game.Scripts.Model.Numbers;
using DG.Tweening;
using GeneralTools.Model;
using UnityEngine;
using UnityEngine.AI;

namespace _Game.Scripts.Model.Unit.Components
{
    public enum MoveState
    {
        None,
        Patrolling,
        MoveToPlayer,
    }
    
    public class MovementComponent : BaseComponent<UnitModel>
    {
        private readonly NavMeshAgent _navMeshAgent;
        private float _range;
        private GameParam _speed;
        private MoveState _moveState;
        private UnitModel _player;
        private List<Transform> _path;
        private int _pathID;
        private Tween _patrolTween;

        public MovementComponent(UnitModel model) : base(model)
        {
            _navMeshAgent = model.View.GetComponent<NavMeshAgent>();
            _speed = model.GetParam(GameParamType.Speed);
            _speed.UpdatedEvent += OnUpdateSpeed;
            _navMeshAgent.stoppingDistance = model.EnemyConfig.NavMeshStopDistance;
            _navMeshAgent.speed = _speed.Value;
            _navMeshAgent.enabled = true;
            if (model.View._patrolling)
            {
                _path = model.View._patrollPoints;
                SetState(MoveState.Patrolling);
            }
        }

        private void OnUpdateSpeed()
        {
            _navMeshAgent.speed = _speed.Value;
        }

        public override void Update(float deltaTime)
        {
            switch (_moveState)
            {
                case MoveState.Patrolling:
                    SetDestination(_path[_pathID].position);
                    if ((_path[_pathID].position - Model.View.transform.position).sqrMagnitude < 0.2f)
                    {
                        NextPoint();
                    }
                    Model.SetTarget();
                    if (Model.State != UnitModel.UnitState.Moving) return;
                    MoveToPlayer();
                    SetState(MoveState.MoveToPlayer);
                    break;
                case MoveState.MoveToPlayer:
                case MoveState.None:
                    Model.SetTarget();
                    if (Model.State != UnitModel.UnitState.Moving) return;
                    SetDestination(_player.View.transform.position);
                    break;
            }

            base.Update(deltaTime);
        }

        private void NextPoint()
        {
            SetState(MoveState.None);
            if (_pathID + 1 < _path.Count)
            {
                _pathID++;
            }
            else
            {
                _pathID = 0;
            }

            _patrolTween = DOTween.Sequence().AppendInterval(Model.EnemyConfig.PatrolEndPointDelay).OnComplete(() =>
            {
                Model.LookAt(_path[_pathID].position, Model.EnemyConfig.PatrolRotationDelay);
                SetDestination(_path[_pathID].position);
                SetState(MoveState.Patrolling);
            });
        }

        public void MoveToPlayer()
        {
            //_range = Model.GetParam(GameParamType.AttackRange).Value;
            _player = Models.Get<GameModel>().Player;
            var position = _player.View.transform.position;
            Model.LookAt(position, Model.EnemyConfig.PatrolRotationDelay);

            SetDestination(position);
            SetState(MoveState.MoveToPlayer);
        }

        public void StopMove()
        {
            _navMeshAgent.enabled = false;
        }

        public void SetState(MoveState moveState)
        {
            _moveState = moveState;
            switch (moveState)
            {
                case MoveState.MoveToPlayer:
                    break;
                case MoveState.Patrolling:
                    break;
            }
        }

        public void KillTween()
        {
            _patrolTween.Kill();
        }

        private void SetDestination(Vector3 position)
        {
            _navMeshAgent.enabled = true;
            NavMeshPath path = new NavMeshPath();
            _navMeshAgent.CalculatePath(position, path);
            _navMeshAgent.SetPath(path);
        }

        public override void End()
        {
            _speed.UpdatedEvent -= OnUpdateSpeed;
            
            base.End();
        }
    }
}