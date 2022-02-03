using _Game.Scripts.Model;
using _Game.Scripts.Model.Items;
using GeneralTools.Model;
using GeneralTools.Tools;
using GeneralTools.UI;
using UnityEngine;

namespace _Game.Scripts.UI.Windows.Inventory
{
    public class InventoryItemActions : BaseUIBehaviour
    {
        [SerializeField] private InventoryItem _inventoryItem;
        [SerializeField] private Transform _container;
        
        [SerializeField] private BaseButton _infoButton;
        [SerializeField] private BaseButton _actionButton;
        [SerializeField] private BaseButton _frameButton;
        [SerializeField] private BaseButton _itemButton;

        private ItemData _item;
        private GameModel _game;
        private InventoryWindow _inventory;

        public void Init()
        {
            _infoButton.SetCallback(OnPressedInfo);
            _actionButton.SetCallback(OnPressedAction);
            _frameButton.Callback = () => this.Deactivate();
            _itemButton.Callback = () => this.Deactivate();

            _game = Models.Get<GameModel>();
            _inventory = GameUI.Get<InventoryWindow>();
        }

        public void SetPosition(Vector3 pos)
        {
            transform.position = pos;
        }

        public void Show(ItemData item)
        {
            _item = item;
            _inventoryItem.Show(item, true);
            _actionButton.SetText(_game.ChosenWeapon(_item) ? "Take off" : "Take on");
            _actionButton.SetInteractable(!_inventory.IsLast(_item));
            if (!_game.ChosenWeapon(_item) && !_inventory.HasPlace())
            {
                _actionButton.SetInteractable(false);
            }
        }
        
        private void OnPressedInfo()
        {
            
        }
        
        private void OnPressedAction()
        {
            if (_game.ChosenWeapon(_item))
            {
                _game.RemoveWeapon(_item);
                _inventory.RemoveWeapon(_item);
            }
            else
            {
                _game.ChooseWeapon(_item);
                _inventory.AddWeapon(_item);
            }

            this.Deactivate();
        }
    }
}