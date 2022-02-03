using _Game.Scripts.Ad;
using _Game.Scripts.Model;
using GeneralTools.Model;
using GeneralTools.UI;
using Plugins.GeneralTools.Scripts.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Game.Scripts.UI.Windows
{
    public class LoseWindow : BaseWindow
    {
        [SerializeField] private BaseButton _continueButton;
        [SerializeField] private BaseButton _adButton;
        
        [SerializeField] private TextMeshProUGUI _timerText;
        [SerializeField] private Image _timerImage;
        
        private GameModel _gameModel;
        private AdModel _ad;
        
        private float _timer;
        private const float _duration = 10;
        private bool _showAd;
        
        public override void Init()
        {
            _gameModel = Models.Get<GameModel>();
            _ad = Models.Get<AdModel>();
            
            _continueButton.SetCallback(OnContinueButtonPressed);
            _adButton.SetCallback(OnAdButtonPressed);
            
            base.Init();
        }

        private void OnAdButtonPressed()
        {
            _showAd = true;
            _ad.AdEvent += OnAdFinished;
            _ad.ShowAd(AdType.LoseWindow);
        }

        private void OnAdFinished(bool success)
        {
            _ad.AdEvent -= OnAdFinished;
            if (!success)
            {
                OnContinueButtonPressed();
            }
            else
            {
                _gameModel.RespawnPlayer();
                Close();   
            }
            _showAd = false;
        }

        public override BaseUI Open()
        {
            _timer = _duration;
            return base.Open();
        }

        public override void UpdateMe(float deltaTime)
        {
            if(_showAd) return;
            _timer -= deltaTime;
            _timerText.text = Mathf.Round(_timer).ToString();
            _timerImage.fillAmount = _timer / _duration;
            
            if (_timer <= 0)
            {
                OnContinueButtonPressed();
            }
            base.UpdateMe(deltaTime);
        }

        private void OnContinueButtonPressed()
        {
            _gameModel.CompleteLevel(false);
            Close();
        }
    }
}