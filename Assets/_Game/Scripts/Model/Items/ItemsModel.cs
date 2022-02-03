using System.Collections.Generic;
using System.Linq;
using _Game.Scripts.Balance;
using _Game.Scripts.Enums;
using _Game.Scripts.Model.Unit;
using _Game.Scripts.View;
using GeneralTools.Model;
using UnityEngine;

namespace _Game.Scripts.Model.Items
{
    public class ItemsModel : BaseModel
    {
        private GameBalance _balance;
        private GameModel _gameModel;
        
        [SerializeField] private List<ItemData> _items;
        [SerializeField] private List<ItemData> _enemyWeapon;

        public List<ItemData> Items => _items;

        public override BaseModel Start()
        {
            
            _balance = GameBalance.Instance;
            _enemyWeapon = new List<ItemData>();
            _gameModel = Models.Get<GameModel>();

            if (_items == null)
            {
                _items = new List<ItemData>();
                
                var item = Add(ItemClass.Weapon, ItemType.Gun);
                Models.Get<GameModel>().ChooseWeapon(item);
            }
            
            _gameModel.CreateProjectile(20, UnitType.Player);
            _gameModel.CreateProjectile(30, UnitType.EnemyBase);
            
            return base.Start();
        }

        public ItemData AddEnemyWeapon(ItemClass itemClass, ItemType weapon)
        {
            var item = new ItemData(itemClass, weapon);
            _enemyWeapon.Add(item);
            FillItemParamValue(item, UnitType.EnemyBase);

            return item;
        }

        public void Add(ItemData itemData)
        {
            _items.Add(itemData);
            FillItemParamValue(itemData, UnitType.Player);
        }

        public ItemData Add(ItemClass itemClass, ItemType type)
        {
            var item = new ItemData(itemClass, type);
            _items.Add(item);
            FillItemParamValue(item, UnitType.Player);
            //_gameModel.CreateProjectile(item.GetParam(GameParamType.MaxAmmunition).IntValue / 2);

            return item;
        }

        private void FillItemParamValue(ItemData item, UnitType type)
        {
            GameBalance.ItemConfig configs = null;
            configs = type == UnitType.Player ? _balance.Items.FirstOrDefault(i => i.Class == item.Class && i.Type == item.Type)
                : _balance.EnemyItems.FirstOrDefault(i => i.Class == item.Class && i.Type == item.Type);
            if (configs == null) return;
            foreach (var config in configs.Params)
            {
                if (config.ParamType == GameParamType.Dmg)
                {
                    var dmgProgression =
                        _balance.Progressions.FirstOrDefault(progression => progression.Type == GameParamType.Dmg);
                    if (dmgProgression != null)
                    {
                        // var level = type == UnitType.Player
                        //     ? _gameModel.CurrentData.GetParam(GameParamType.PlayerLevel).Level
                        //     : _gameModel.CurrentData.Region;
                        // item.FillItemParam(config.ParamType, (float)dmgProgression.GetValue(level, config.Value));
                        if (type == UnitType.Player)
                        {
                            var level = _gameModel.CurrentData.GetParam(GameParamType.PlayerLevel).Level;
                                // ? _gameModel.CurrentData.GetParam(GameParamType.PlayerLevel).Level
                                // : _gameModel.CurrentData.Region;
                            item.FillItemParam(config.ParamType, (float)dmgProgression.GetValue(level, config.Value));   
                        }
                        else
                        {
                            item.FillItemParam(config.ParamType, config.Value); 
                        }
                    }
                }
                else
                {
                    item.FillItemParam(config.ParamType, config.Value); 
                }
            }
        }

        public void CopyFrom(ItemsModel source)
        {
            _items = source._items;
            foreach (var item in _items)
            {
                item.CopyFrom();
            }
        }

    }
}