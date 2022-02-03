using System;
using System.Globalization;
using _Game.Scripts.Enums;
using _Game.Scripts.Model;
using _Game.Scripts.Model.Items;
using _Game.Scripts.UI.Windows.MainGame;
using DG.Tweening;
using GeneralTools.Model;
using GeneralTools.Tools;
using GeneralTools.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Game.Scripts.UI.HUD
{
    public class MainGameItem : BaseUIBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private TextMeshProUGUI _amount;
        [SerializeField] private Slider _slider;
        [SerializeField] private BaseButton _button;
        [SerializeField] private Image _image;
        [SerializeField] private Color _equipColor;

        private GameModel _game;
        private MainGameWindow _mainGame;
        private ItemData _itemData;
        private Tween _reloadTween;

        public event Action<ItemData> OnEquipItem;
        public ItemData ItemData => _itemData;
        
        public void Init()
        {
            _game = Models.Get<GameModel>();
            _mainGame = GameUI.Get<MainGameWindow>();
            _button.SetCallback(OnPressedButton);
        }
        
        public void Show(ItemData item)
        {
            if (item == null)
            {
                _icon.Deactivate();
                _slider.Deactivate();
                if (_itemData is { Class: ItemClass.Weapon })
                {
                    _itemData.Ammunition.UpdatedEvent -= Redraw;
                }

                _itemData = null;
                return;   
            }

            _icon.sprite = item.Type.GetSprite();
            _icon.Activate();
            _slider.Activate();
            
            _itemData = item;
            if (item.Class == ItemClass.Weapon)
            {
                item.Ammunition.UpdatedEvent += Redraw;
            }
            Redraw();
        }

        public void SetReload(bool value)
        {
            if (value)
            {
                _reloadTween = DOTween.To(x => _slider.value = x, 0, 1, _itemData.Config.ReloadAnimationDuration);
            }
            else
            {
                _reloadTween?.Kill();
                Redraw();
            }
        }

        public void Redraw()
        {
            if (_itemData == null) return;
            var amount = _itemData.GetParam(GameParamType.Ammunition, false);
            if (amount == null)
            {
                _amount.text = "";
                _slider.value = 0;
            }
            else
            {
                _amount.text = amount.Value.ToString(CultureInfo.InvariantCulture);
                _slider.value = (float) amount.IntValue / _itemData.GetParam(GameParamType.MaxAmmunition, false).IntValue;   
            }
        }

        public void SetColor(bool equiped)
        {
            if (equiped)
            {
                _image.color = _equipColor;
            }
            else
            {
                _image.color = Color.white;   
            }
        }

        private void OnPressedButton()
        {
            if(_itemData == null) return;
            _game.Player.EquipWeapon(_itemData);
            _mainGame.RedrawItems();
            SetColor(true);
            OnEquipItem?.Invoke(_itemData);
        }
    }
}