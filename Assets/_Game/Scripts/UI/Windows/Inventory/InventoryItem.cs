using _Game.Scripts.Enums;
using _Game.Scripts.Model.Items;
using GeneralTools.Tools;
using GeneralTools.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Game.Scripts.UI.Windows.Inventory
{
    public class InventoryItem : BaseUIBehaviour
    {
        [SerializeField] private TextMeshProUGUI _name;
        [SerializeField] private TextMeshProUGUI _level;
        [SerializeField] private TextMeshProUGUI _paramText;
        [SerializeField] private Image _icon;
        [SerializeField] private Transform _activeContainer;
        [SerializeField] private Transform _emptyContainer;
        [SerializeField] private BaseButton _button;
        
        public ItemData Item { get; private set; }

        public void Show(ItemData item, bool readOnly = false)
        {
            if (item == null)
            {
                ShowEmpty();
                return;
            }

            _icon.sprite = item.Type.GetSprite();
            
            if (!readOnly) _button.SetCallback(OnPressedButton);
            _button.SetInteractable(!readOnly);
            
            Item = item;
            _name.text = item.Type.ToString();
            _level.text = $"Lvl.:{item.Level}";
            _paramText.text = $"DMG:{item.GetParam(GameParamType.Dmg).IntValue}";

            _activeContainer.Activate();
            _emptyContainer.Deactivate();
        }

        public void ShowEmpty()
        {
            Item = null;
            _button.Callback = null;
            _button.SetInteractable(false);
            _activeContainer.Deactivate();
            _emptyContainer.Activate();
        }

        private void OnPressedButton()
        {
            GameUI.Get<InventoryWindow>().ShowItemActions(this);
        }
    }
}