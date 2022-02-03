using System;
using System.Collections.Generic;
using System.Linq;
using _Game.Scripts.Balance;
using _Game.Scripts.Enums;
using _Game.Scripts.Model;
using _Game.Scripts.Model.Drop;
using _Game.Scripts.Model.Items;
using _Game.Scripts.Model.Numbers;
using _Game.Scripts.UI.Progress;
using DG.Tweening;
using GeneralTools.Model;
using GeneralTools.Tools;
using Plugins.GeneralTools.Scripts.View;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace _Game.Scripts.View.Drop
{
    public class LootBox : ViewWithModel<DropModel>
    {
        [SerializeField] private ParticleSystem _conffeti;
        [SerializeField] private Animation _animation;
        [SerializeField] private float _animationDelay;
        [SerializeField] private SliderProgressUI _slider;
        [SerializeField] private ParticleSystem _particleSystem;
        [SerializeField] private bool _prepareLoot;
        [SerializeField] private Collider _caseCollider;
        [SerializeField] private int _moneyPack;
        [SerializeField] private int _baseMoneyPerPack;
        [ShowIf("_prepareLoot")][SerializeField] private ItemClass _lootClass;
        [ShowIf("_prepareLoot")][SerializeField] private ItemType _lootType;
        private GameProgress _openProgress;
        private GameModel _gameModel;
        private bool _opened;
        private DropContainer _dropContainer;

        public event Action OnFirstTrigger;

        public override void StartMe()
        {
            _openProgress = new GameProgress(GameParamType.Timer, 
                GameBalance.Instance.LootBoxOpenDelay, false);
            _openProgress.Pause();
            _openProgress.CompletedEvent += OpenChest;
            _gameModel = Models.Get<GameModel>();
            _dropContainer = Models.Get<DropContainer>();

            _slider.SetProgress(_openProgress);
            
            base.StartMe();
        }

        public override void UpdateMe(float deltaTime)
        {
            _openProgress.Change(deltaTime);
            
            base.UpdateMe(deltaTime);
        }

        private void OpenChest()
        {
            _opened = true;
            _slider.Deactivate();
            _openProgress.CompletedEvent -= OpenChest;
            _animation.Play();
            DOTween.Sequence().AppendInterval(_animationDelay).OnComplete(DropLoot);
        }

        private void DropLoot()
        {
            _particleSystem.Stop();
            _caseCollider.enabled = false;
            
            var moneyProgression = GameBalance.Instance.Progressions.FirstOrDefault(progression =>
                progression.Type == GameParamType.SoftPerPack);
            int softPerPack = 0;
            if (moneyProgression != null)
            {
                softPerPack = (int)moneyProgression.GetValue(Models.Get<GameModel>().CurrentData.Region, _baseMoneyPerPack);
            }

            DropModel drop;
            for (int i = 0; i < _moneyPack; i++)
            {
                drop = _dropContainer.SpawnDrop(ItemClass.Value, ItemType.Soft);
                drop.Count = softPerPack;
                drop.Jump(transform.position);
            }
            
            var playerLevel = _gameModel.CurrentData.GetParam(GameParamType.PlayerLevel).IntValue;
            IEnumerable<GameBalance.DropConfig> dropList = new List<GameBalance.DropConfig>();
            if (_prepareLoot)
            {
                dropList = GameBalance.Instance.DropConfigs.Where(drop => drop.Level <= playerLevel && drop.ItemClass == _lootClass &&
                                                                          drop.Type == _lootType);
            }
            else
            {
                dropList = GameBalance.Instance.DropConfigs.Where(drop => drop.Level <= playerLevel && (drop.ItemClass == ItemClass.Weapon
                    || drop.ItemClass == ItemClass.MeleeWeapon || drop.ItemClass == ItemClass.Grenade));   
            }
            
            var rndDropConfig = dropList.GetRandomItem();
            drop = Models.Get<DropContainer>().SpawnDrop(rndDropConfig.ItemClass, rndDropConfig.Type);
            drop.Jump(transform.position);
            GameSounds.Instance.PlaySound(GameSoundType.Runduk);
            _conffeti.Play();
            //DOTween.Sequence().AppendInterval(GameBalance.Instance.CollectDelay).OnComplete(Models.Get<DropContainer>().CollectAll);
        }

        private void OnTriggerEnter(Collider other)
        {
            if(_opened) return;
            if (other.gameObject.layer == 10)
            {
                OnFirstTrigger?.Invoke();
                _slider.Activate();
                _openProgress.Play();   
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if(_opened) return;
            if (other.gameObject.layer == 10)
            {
                _slider.Deactivate();
                _openProgress.SetValue(0);
                _openProgress.Pause();
            }
        }
    }
}