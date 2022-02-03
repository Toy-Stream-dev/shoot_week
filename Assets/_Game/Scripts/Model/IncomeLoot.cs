using System;
using System.Collections.Generic;
using _Game.Scripts.Enums;
using _Game.Scripts.Model.Items;
using _Game.Scripts.Model.Numbers;
using DG.Tweening;
using GeneralTools.Model;
using UnityEngine;

namespace _Game.Scripts.Model
{
    public enum LootType
    {
        None,
        Soft,
        Hard,
        Weapon,
        Experience,
    }
    
    public class IncomeLoot : BaseModel
    {
        private GameModel _gameModel;
        private List<ItemData> _weapons = new List<ItemData>();
        private int _soft;
        private int _hard;
        private int _exp;

        public int Experience => _exp;
        public int Hard => _hard;
        public int Soft => _soft;
        public List<ItemData> Weapons => _weapons;
        public event Action OnGetItem;

        public IncomeLoot(GameModel gameModel)
        {
            _gameModel = gameModel;
        }

        public void Push(LootType lootType, int value)
        {
            switch (lootType)
            {
                case LootType.Soft:
                    _gameModel.AddMoney(value);
                    GameSounds.Instance.PlaySound(GameSoundType.Soft);
                    _soft += value;
                    break;
                case LootType.Hard:
                    _hard += value;
                    break;
                case LootType.Experience:
                    _exp += value;
                    break;
            }
        }

        public void Push(LootType lootType, ItemData item)
        {
            switch (lootType)
            {
                case LootType.Weapon:
                    _weapons.Add(item);
                    OnGetItem?.Invoke();
                    break;
            }
        }

        public void ConfirmAll()
        {
            _gameModel.CurrentData.IncreaseExperience(_exp);
            //_gameModel.AddMoney(_soft);
            _gameModel.CurrentData.GetParam(GameParamType.Hard).Change(_hard);
            foreach (var weapon in _weapons)
            {
                Models.Get<ItemsModel>().Add(weapon);   
            }
        }
        
        public void ClearAll()
        {
            _soft = 0;
            _hard = 0;
            _exp = 0;
            _weapons.Clear();
        }
    }
}