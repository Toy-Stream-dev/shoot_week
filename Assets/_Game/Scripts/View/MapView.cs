using System;
using System.Collections.Generic;
using System.Linq;
using _Game.Scripts.Model;
using _Game.Scripts.UI.Windows.MainGame;
using _Game.Scripts.View.Drop;
using GeneralTools.Model;
using GeneralTools.UI;
using Plugins.GeneralTools.Scripts.View;
using UnityEngine;

namespace _Game.Scripts.View
{
    public class MapView : ViewWithModel<MapModel>
    {
        [SerializeField] private Transform _playerSpawnPoint;
        [SerializeField] private Transform _storePlayerSpawnPoint;

        private Door _door;
        private CompleteLevelCollider _completeLevelCollider;
        private List<LootBox> _lootBoxes = new List<LootBox>();
        private List<DropView> _dropViews = new List<DropView>();
        private GameModel _gameModel;

        public UnitView Boss;
        public List<DropView> DropViews => _dropViews;
        public Vector3 PlayerPosition => _playerSpawnPoint.position;
        public Vector3 StorePlayerPosition => _storePlayerSpawnPoint.position;
        
        public void Init(Transform parent)
        {
            _lootBoxes = GetComponentsInChildren<LootBox>().ToList();
            _gameModel = Models.Get<GameModel>();
            foreach (var lootBox in _lootBoxes)
            {
                lootBox.StartMe();
                lootBox.OnFirstTrigger += OnLootBoxFirstTrigger;
            }

            Boss = GetComponentsInChildren<UnitView>().ToList().FirstOrDefault(unit => unit.Boss);
            _dropViews = GetComponentsInChildren<DropView>().ToList();
            _door = GetComponentInChildren<Door>();
            _completeLevelCollider = GetComponentInChildren<CompleteLevelCollider>();
            
            if (_completeLevelCollider == null)
            {
                Debug.LogError($"Can`t find CompleteLevelCollider");
            }
            else
            {
                _completeLevelCollider.StartMe();   
            }

            if (_door != null)
            {
                _door.StartMe();   
            }
            
            transform.SetParent(parent);
            base.Init();
        }

        public void OnLootBoxFirstTrigger()
        {
            foreach (var lootBox in _lootBoxes)
            {
                lootBox.OnFirstTrigger -= OnLootBoxFirstTrigger;
            }
            _gameModel.EnemyContainer.GoToAttackAll();
        }
            
        public override void UpdateMe(float deltaTime)
        {
            foreach (var lootBox in _lootBoxes)
            {
                lootBox.UpdateMe(deltaTime);
            }
            base.UpdateMe(deltaTime);
        }
    }
}