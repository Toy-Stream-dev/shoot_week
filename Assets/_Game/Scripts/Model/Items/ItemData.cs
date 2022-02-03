using System;
using System.Collections.Generic;
using System.Linq;
using _Game.Scripts.Balance;
using _Game.Scripts.Enums;
using _Game.Scripts.Model.Numbers;
using _Game.Scripts.Model.Unit;
using _Game.Scripts.Saves;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Game.Scripts.Model.Items
{
    [Serializable]
    public class ItemData
    {
        [SerializeField] private bool _chosen;
        [SerializeField] private GameBalance _balance;
        [SerializeField] private GameBalance.ItemConfig _config;
        [SerializeField] private GameSoundType _soundType;
        [SerializeField] private int _bulletPerShot;
        [SerializeField] private List<GameParam> _params = new List<GameParam>();
        [SerializeField] private int _level;
        [SerializeField] private float _maxScatter;
        [SerializeField] private float _minScatter;
        [SerializeField] private ItemType _type;
        [SerializeField] private ItemClass _class;
        private GameParam _ammunition;
        private GameParam _maxAmmunition;

        public bool Chosen => _chosen;
        public ItemClass Class => _class;
        public ItemType Type => _type;
        public float MinScatter => _minScatter;
        public float MaxScatter => _maxScatter;
        public int Level => _level;
        public GameBalance.ItemConfig Config => _config;
        public int BulletPerShot => _bulletPerShot;
        public GameSoundType SoundType => _soundType;
        public GameParam Ammunition => _ammunition;
        public GameParam MaxAmmunition => _maxAmmunition;

        public void SetChosen(bool value)
        {
            _chosen = value;
        }
        
        public ItemData(ItemClass itemClass, ItemType type)
        {
            _balance = GameBalance.Instance;
            _config = _balance.Items.FirstOrDefault(i => i.Class == itemClass && i.Type == type);
            if (_config != null && type == ItemType.Shotgun)
            {
                _bulletPerShot = _config.BulletPerShoot;
                _minScatter = _config.MinScatter;
                _maxScatter = _config.MaxScatter;
            }

            if (_config != null) _soundType = _config.SoundType;
            _class = itemClass;
            _type = type;
            
            Init();
        }

        private void Init()
        {
            switch (_class)
            {
                case ItemClass.MedPack:
                    CreateParam(GameParamType.MedPack);
                    break;
                
                case ItemClass.Weapon:
                    CreateParam(GameParamType.Dmg);
                    CreateParam(GameParamType.AttackSpeed);
                    CreateParam(GameParamType.AttackRange);
                    _maxAmmunition = CreateParam(GameParamType.MaxAmmunition);
                    _ammunition = CreateParam(GameParamType.Ammunition);
                    CreateParam(GameParamType.AnimationWeaponID);
                    break;
                case ItemClass.MeleeWeapon:
                    CreateParam(GameParamType.Dmg);
                    CreateParam(GameParamType.AttackSpeed);
                    CreateParam(GameParamType.AttackRange);
                    CreateParam(GameParamType.AnimationWeaponID);
                    break;
                case ItemClass.Grenade:
                    CreateParam(GameParamType.Dmg);
                    CreateParam(GameParamType.AttackRange);
                    CreateParam(GameParamType.ExplosionRadius);
                    CreateParam(GameParamType.ExplosionDelay);
                    CreateParam(GameParamType.AnimationWeaponID);
                    _ammunition = CreateParam(GameParamType.Ammunition);
                    _maxAmmunition = CreateParam(GameParamType.MaxAmmunition);
                    break;
            }
        }

        public void ResetAmmunition()
        {
            if (Class == ItemClass.Weapon)
            {
                _ammunition.SetValue(_maxAmmunition.Value);
            }
        }

        public bool HasParam(GameParamType type)
        {
            var param = _params.Find(p => p.Type == type);
            if (param != null) return true;
            return false;
        }
        
        public void FillItemParam(GameParamType type, float value)
        {
            GetParam(type).SetValue(value);
        }
        
        private GameParam CreateParam(GameParamType type)
        {
            var param = GetParam(type);
            return param;
        }
        
        public GameParam GetParam(GameParamType type, bool createIfNotExists = true)
        {
            var param = _params.Find(p => p.Type == type);
            if (param != null || !createIfNotExists) return param;
            param = new GameParam(type);
            _params.Add(param);
            return param;
        }

        public void CopyFrom()
        {
            _ammunition = GetParam(GameParamType.Ammunition);
            _maxAmmunition = GetParam(GameParamType.MaxAmmunition);
        }
    }

    [Serializable]
    public enum ItemClass
    {
        None,
        MedPack,
        Weapon,
        MeleeWeapon,
        Grenade,
        Value,
        Mask,
    }

    [Serializable]
    public enum ItemType
    {
        Gun,
        AutomaticRifle,
        Uzi,
        Shotgun,
        BaseballBat,
        FragGrenade,
        Soft,
        Hard,
        Crowbar,
        Baton,
        Katana,
        Machete,
        Cleaver,
        SmallHealth,
        Magnum,
    }
}