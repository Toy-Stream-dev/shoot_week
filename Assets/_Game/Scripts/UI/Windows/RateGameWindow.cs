using System.Collections.Generic;
using GeneralTools.Localization;
using GeneralTools.Tools;
using GeneralTools.UI;
using Plugins.GeneralTools.Scripts.UI;
using TMPro;
using UnityEngine;

namespace _Game.Scripts.UI.Windows
{
    public class RateGameWindow : BaseWindow
    {
        [SerializeField] private TextMeshProUGUI _title, _starsText, _text;
        [SerializeField] private BaseButton _btnRate;
        [SerializeField] private List<BaseButton> _stars;
        
        private int _starCount;
        
        public override void Init()
        {
            foreach (var star in _stars)
            {
                star.SetCallback(() => OnPressedStar(star));
            }
            
            _btnRate.SetCallback(OnPressedRate);

            base.Init();
        }

        public override BaseUI Open()
        {
            _title.text = "rate_title".Localized();
            _text.text = "rate_text".Localized();
            _starsText.text = "rate_stars_text".Localized();
            _btnRate.SetText("rate_button".Localized());
            
            _starCount = 4;
            PlayerPrefs.SetString("IsAppRatedShowed", "");
            RedrawStars();
            return base.Open();
        }

        protected override void OnPressedClose()
        {
            AppEventsProvider.TriggerEvent(GameEvents.Rate_us, 0);
            base.OnPressedClose();
        }

        private void OnPressedStar(BaseButton _btn)
        {
            _starCount = _stars.IndexOf(_btn);
            RedrawStars();
        }

        private void RedrawStars()
        {
            for (int i = 0; i < _stars.Count; i++)
            {
                var sprite = i <= _starCount ? "star".GetSprite() : "star_grey".GetSprite();
                _stars[i].Sprite = sprite;
            }
        }

        private void OnPressedRate()
        {
            if (_starCount == 4)
            {
                Application.OpenURL("");
            }
            
            AppEventsProvider.TriggerEvent(GameEvents.Rate_us, _starCount + 1);
            PlayerPrefs.SetString("IsAppRated", "");
            Close();
        }
    }
}