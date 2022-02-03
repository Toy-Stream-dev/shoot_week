using System.Collections.Generic;
using System.Linq;
using _Game.Scripts.Balance;
using _Game.Scripts.Enums;
using _Game.Scripts.Model;
using _Game.Scripts.Model.Items;
using _Game.Scripts.UI.Progress;
using GeneralTools.Model;
using GeneralTools.Pooling;
using GeneralTools.Tools;
using GeneralTools.UI;
using Plugins.GeneralTools.Scripts.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Game.Scripts.UI.Windows.Inventory
{
    public class InventoryWindow : BaseWindow
    {
        [SerializeField] private Transform _itemsContainer;
        [SerializeField] private InventoryItemActions _itemActions;
        [SerializeField] private List<InventoryItem> _weapons;
        [SerializeField] private TextMeshProUGUI _level;
        [SerializeField] private TextMeshProUGUI _exp;
        [SerializeField] private SliderProgressUI _expSlider;
        [SerializeField] private TextMeshProUGUI _soft;
        [SerializeField] private TextMeshProUGUI _hard;
        
        private readonly List<InventoryItem> _items = new List<InventoryItem>();

        private GameModel _game;
        private ItemsModel _itemModel;
        
        public override void Init()
        {
            _game = Models.Get<GameModel>();
            _itemModel = Models.Get<ItemsModel>();
            Pool.Spawn<InventoryItem>(25);
            _expSlider.SetProgress(_game.CurrentData.GetProgress(GameParamType.PlayerLevel));

            _itemActions.Init();
            _itemActions.Deactivate();
            
            base.Init();
        }

        public override BaseUI Open()
        {
            _soft.text = _game.CurrentData.GetParam(GameParamType.Soft).Value.ToString();
            _hard.text = _game.CurrentData.GetParam(GameParamType.Hard).Value.ToString();
            _level.text = _game.CurrentData.GetParam(GameParamType.PlayerLevel).Level.ToString();

            var exp = _game.CurrentData.GetProgress(GameParamType.PlayerLevel);
            _exp.text = $"{exp.CurrentValue}/{exp.Target}";
            
            for (int i = 0; i < _weapons.Count; i++)
            {
                _weapons[i].Show(i < _game.ChosenWeapons.Count ? _game.ChosenWeapons[i] : null);
            }

            RedrawItems();

            return base.Open();
        }

        public bool IsLast(ItemData itemData)
        {
            int countWeapon = 0;
            bool hasEquip = false; 
            foreach (var weapon in _weapons)
            {
                if (weapon.Item != null)
                {
                    countWeapon++;
                }

                if (weapon.Item == itemData)
                {
                    hasEquip = true;
                }
            }

            return countWeapon == 1 && hasEquip;
        }

        public bool HasPlace()
        {
            foreach (var weapon in _weapons)
            {
                if (weapon.Item == null)
                {
                    return true;
                }
            }

            return false;
        }

        private void RedrawItems()
        {
            _items.PushAllToPoolAndClear();
            foreach (var item in _itemModel.Items)
            {
                var curr = _weapons.FirstOrDefault(c => c.Item == item);
                if (curr != null) continue;

                var newItem = Pool.Pop<InventoryItem>(_itemsContainer);
                newItem.Show(item);
                _items.Add(newItem);
                GameUI.Get<HUD.HUD>().AddGameplayElement(newItem.GetComponent<GamePlayElementUI>());
            }
        }

        public void AddWeapon(ItemData item)
        {
            var element = _weapons.FirstOrDefault(i => i.Item == null);
            if (element == null) return;
            element.Show(item);

            RedrawItems();
        }
        
        public void RemoveWeapon(ItemData item)
        {
            var element = _weapons.FirstOrDefault(i => i.Item == item);
            if (element == null) return;
            element.ShowEmpty();

            RedrawItems();
        }

        public override void Close()
        {
            _items.PushAllToPoolAndClear();
            base.Close();
        }

        public void ShowItemActions(InventoryItem item)
        {
            var position = item.rectTransform.position;
            _itemActions.SetPosition(new Vector3(position.x, position.y - 50, position.z));
            _itemActions.Show(item.Item);
            _itemActions.Activate();
        }
    }
}