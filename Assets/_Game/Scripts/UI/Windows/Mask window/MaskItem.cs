using _Game.Scripts.Balance;
using _Game.Scripts.Enums;
using _Game.Scripts.Model;
using _Game.Scripts.UI.Message;
using _Game.Scripts.UI.Windows.MainGame;
using GeneralTools;
using GeneralTools.Model;
using GeneralTools.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Game.Scripts.UI.Windows.Mask_window
{
    public class MaskItem : BaseBehaviour
    {
        [SerializeField] private TextMeshProUGUI _bonusText;
        [SerializeField] private TextMeshProUGUI _price;
        [SerializeField] private Image _icon;
        [SerializeField] private BaseButton _button;
        [SerializeField] private TextMeshProUGUI _duration;

        private GameBalance.MaskConfig _mask;
        private GameModel _gameModel;
        
        public void Show(GameBalance.MaskConfig mask)
        {
            _button.SetCallback(OnPressedBuy);
            _mask = mask;
            _bonusText.text = mask.Text;
            _price.text = mask.Price.ToString();
            _icon.sprite = mask.Image.GetSprite();
            _duration.text = $"{mask.Duration} sec";
            _gameModel = Models.Get<GameModel>();
        }

        private void OnPressedBuy()
        {
            var game = Models.Get<GameModel>();
            var soft = game.CurrentData.GetParam(GameParamType.Soft).Value;
            if (soft >= _mask.Price)
            {
                game.SpendMoney(_mask.Price);
                game.Player.EquipMask(_mask);
                GameUI.Get<ChooseMaskWindow>().Close();
                //GameUI.Get<MainGameWindow>().SetMask(_mask);
            }
            else
            {
                GameUI.Get<MessageContainer>().Show("Not enough cash.");
            }
        }

        public void Redraw()
        {
            _button.SetInteractable(_gameModel.IsEnough(GameParamType.Soft, _mask.Price));
        }
    }
}