using System;
using System.Collections.Generic;
using _Game.Scripts.Balance;
using _Game.Scripts.Model;
using _Game.Scripts.Model.Unit;
using DG.Tweening;
using GeneralTools;
using GeneralTools.Model;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Game.Scripts.View
{
    public class Door : BaseBehaviour
    {
        [SerializeField] private Animation _animation;
        [SerializeField] private List<Collider> _colliders;
        [SerializeField] private bool _opened;
        [SerializeField] private Transform _kickPoint;
        [SerializeField] private ParticleSystem _area;


        private GameModel _gameModel;
        private bool _awaitLoot;

        public bool Opened => _opened;

        public override void StartMe()
        {
            _gameModel = Models.Get<GameModel>();
            _gameModel.LevelCleared += OnLevelCleared;
            if (_gameModel.CurrentData.Region == GameBalance.Instance.TutorialLevelID)
            {
                _awaitLoot = true;
                _gameModel.IncomeLoot.OnGetItem += OnGetItem;
            }
            
            base.StartMe();
        }

        private void OnGetItem()
        {
            _awaitLoot = false;
            _gameModel.IncomeLoot.OnGetItem -= OnGetItem;
            if (_area.isStopped)
            {
                _area.Play();
            }
        }
        
        private void OnLevelCleared()
        {
            _gameModel.LevelCleared -= OnLevelCleared;
            if (_area != null && !_awaitLoot)
            {
                _area.Play();   
            }
        }

        public void OnTriggerEnter(Collider other)
        {
            if (!_opened && other.gameObject.layer == 10 && _gameModel.LevelClear && !_awaitLoot)
            {
                var unit = other.gameObject.GetComponent<UnitView>();
                if (unit.Type == UnitType.Player)
                {
                    unit.Model.KickDoor(this, _kickPoint.position);
                    _opened = true;
                }
            }
        }

        [Button]
        public void PlayAnimation()
        {
            _animation.Play();
        }
        
        public void Open()
        {
            PlayAnimation();
            _area.Stop();
            foreach (var collider in _colliders)
            {
                collider.enabled = false;
            }

            DOTween.Sequence().AppendInterval(1.5f).OnComplete(() =>  _gameModel.CompleteLevel(true));
        }
    }
}