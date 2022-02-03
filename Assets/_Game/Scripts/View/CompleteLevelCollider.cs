using System;
using System.Collections.Generic;
using _Game.Scripts.Model;
using _Game.Scripts.Model.Unit;
using GeneralTools;
using GeneralTools.Model;
using UnityEngine;

namespace _Game.Scripts.View
{
    public class CompleteLevelCollider : BaseBehaviour
    {
        private GameModel _gameModel;
        private bool _levelCompleted;

        public override void StartMe()
        {
            _gameModel = Models.Get<GameModel>();
            
            base.StartMe();
        }

        private void OnTriggerEnter(Collider other)
        {
            if(_levelCompleted) return;
            if (other.gameObject.layer == 10 && _gameModel.LevelClear)
            {
                _gameModel.CompleteLevel(true);
                _levelCompleted = true;
            }
        }
    }
}