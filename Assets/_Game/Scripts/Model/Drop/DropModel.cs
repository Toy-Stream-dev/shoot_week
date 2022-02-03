using System;
using System.Linq;
using _Game.Scripts.Balance;
using _Game.Scripts.Enums;
using _Game.Scripts.Model.Items;
using _Game.Scripts.Model.Unit;
using _Game.Scripts.UI.Windows.MainGame;
using _Game.Scripts.View.Drop;
using DG.Tweening;
using GeneralTools.Model;
using GeneralTools.UI;
using UnityEngine;
using UnityEngine.Playables;
using Random = GeneralTools.Tools.Random;

namespace _Game.Scripts.Model.Drop
{
    public enum DropType
    {
        None,
        Value,
        Weapon,
    }

    public enum DropState
    {
        None,
        Jump,
        Lie,
    }
    
    public class DropModel : ModelWithView<DropView>
    {
        private GameBalance.DropConfig _dropConfig;
        private DropState _state;
        private GameModel _gameModel;
        private bool _collect;
        private UnitModel Player;
        private float _collectSpeed;
        
        public int Count { get; set; }

        public Tween _collectTween;

        public GameBalance.DropConfig DropConfig => _dropConfig;
        
        public new DropModel Start()
        {
            _gameModel = Models.Get<GameModel>();
            _collectSpeed = GameBalance.Instance.CollectSpeed;
            var config = GameBalance.Instance.DropConfigs.FirstOrDefault(drop => drop.ItemClass == View.Class);
            if (config != null)
            {
                _dropConfig = config;
            }
            else
            {
                Debug.LogError($"Can`t find DropConfig with type {View.Class}");
            }
            
            base.Start();
            return this;
        }

        public override void Update(float deltaTime)
        {
            if(!_collect) return;
            var newPos = Vector3.MoveTowards(View.transform.position,
                Player.View.BulletPoint.transform.position, _collectSpeed * deltaTime);
            View.transform.position = newPos;
            if ((newPos - Player.View.BulletPoint.transform.position).sqrMagnitude < 0.01f)
            {
                _collect = false;
                Execute();
            }
            
            base.Update(deltaTime);
        }

        public new DropModel SpawnView(Transform root, bool activate, Predicate<DropView> predicate = default)
        {
            base.SpawnView(root, activate, predicate);
            return this;
        }

        public void SetPosition(Vector3 position)
        {
            View.transform.position = position;
        }

        public void Jump(Vector3 startPos)
        {
            SetState(DropState.Jump);
            SetPosition(startPos);
            var rndX = Random.Range(-_dropConfig.DropRange, _dropConfig.DropRange);
            var rndZ = Random.Range(-_dropConfig.DropRange, _dropConfig.DropRange);
            var target = new Vector3(startPos.x + rndX, startPos.y, startPos.z + rndZ);
            var sequence = DOTween.Sequence();
            sequence.Append(View.transform.DOJump(target, _dropConfig.JumpForce, 1, _dropConfig.JumpTime));
            sequence.Join(View.transform.DORotate(new Vector3(0, Random.Range(0, 360),
                0), _dropConfig.JumpTime)).OnComplete(() => SetState(DropState.Lie));
        }

        private void SetState(DropState state)
        {
            if(_state == state) return;
            _state = state;
            switch (_state)
            {
                case DropState.Jump:
                    View.OnJump();
                    break;
                case DropState.Lie:
                    _collectTween = DOTween.Sequence().AppendInterval(GameBalance.Instance.CollectDelay).OnComplete(Collect);
                    View.OnLie();
                    break;
            }
        }

        public void KillTween()
        {
            _collectTween?.Kill();
        }

        public void Collect()
        {
            Player = _gameModel.Player;
            _collect = true;
        }

        public void Execute()
        {
            KillTween();
            if (View.Class == ItemClass.Mask)
            {
                var maskConfig = GameBalance.Instance.Masks.FirstOrDefault(mask => mask.Type == View.MaskType);
                if (maskConfig == null)
                {
                    Debug.LogError($"Can`t find MaskConfig for {View.MaskType}");
                    return;
                }
                GameSounds.Instance.PlaySound(GameSoundType.PickUP);
                _gameModel.Player.EquipMask(maskConfig);
            }
            else
            {
                switch (View.ItemType)
                {
                    case ItemType.Soft:
                        _gameModel.IncomeLoot.Push(LootType.Soft, Count);
                        break;
                    case ItemType.Gun:
                    case ItemType.AutomaticRifle:
                    case ItemType.Shotgun:
                    case ItemType.BaseballBat:
                    case ItemType.FragGrenade:
                    case ItemType.Uzi:
                    case ItemType.Baton:
                    case ItemType.Cleaver:
                    case ItemType.Crowbar:
                    case ItemType.Machete:
                    case ItemType.Magnum:
                    case ItemType.Katana:
                        GameSounds.Instance.PlaySound(GameSoundType.PickUP);
                        _gameModel.IncomeLoot.Push(LootType.Weapon, new ItemData(View.Class, View.ItemType));
                        GameUI.Get<MainGameWindow>().ShowInfo(View.ItemType.ToString());
                        break;
                    case ItemType.SmallHealth:
                        GameSounds.Instance.PlaySound(GameSoundType.MedPack);
                        _gameModel.Player.Health(ItemType.SmallHealth);
                        break;
                }
            }

            Models.Get<DropContainer>().Destroy(this);
        }

        public void Destroy()
        {
            DestroyView();
        }
    }
}