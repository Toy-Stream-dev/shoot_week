using System.Collections.Generic;
using System.Threading.Tasks;
using _Game.Scripts.Ad;
using _Game.Scripts.Balance;
using _Game.Scripts.Enums;
using _Game.Scripts.Model;
using GeneralTools.Model;
using GeneralTools.Pooling;
using GeneralTools.Tools;
using GeneralTools.UI;
using Plugins.GeneralTools.Scripts.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Game.Scripts.UI.Windows
{
    public class WinWindow : BaseWindow
    {
        [SerializeField] private BaseButton _nextButton;
        [SerializeField] private Image _titleBanner;
        [SerializeField] private TextMeshProUGUI _title;
        [SerializeField] private TextMeshProUGUI _moneyAmount;
        [SerializeField] private Image _resultImage;
        [SerializeField] private Sprite _winSprite;
        [SerializeField] private Sprite _loseSprite;
        [SerializeField] private TextMeshProUGUI _nextButtonText;
        [SerializeField] private RectTransform _lootContainer;

        private List<LootItemUI> _lootItems = new List<LootItemUI>();

        private string CONTINUE = "CONTINUE";
        private string RESTART = "RESTART";

        private GameModel _gameModel;
        private AdModel _ad;
        private bool _isWin;

        public override void Init()
        {
            _gameModel = Models.Get<GameModel>();
            _ad = Models.Get<AdModel>();
            _nextButton.SetCallback(OnNextButtonPressed);
            Pool.Spawn<LootItemUI>(5);
            
            base.Init();
        }

        public void Open(bool isWin)
        {
            _nextButton.Deactivate();
            _isWin = isWin;
            _resultImage.sprite = isWin ? _winSprite : _loseSprite;
            _nextButtonText.text = isWin ? CONTINUE : RESTART;
            _title.text = isWin ? "Victory" : "Defeat";
            _moneyAmount.text = _gameModel.IncomeLoot.Soft.ToString();
            SpawnLoot();
            base.Open();
        }

        private async void SpawnLoot()
        {
            if (_gameModel.IncomeLoot.Weapons != null)
            {
                foreach (var weapon in _gameModel.IncomeLoot.Weapons)
                {
                    var loot = Pool.Pop<LootItemUI>(_lootContainer);
                    _lootItems.Add(loot);
                    loot.SetSprite(weapon.Type.ToString());
                    loot.SetText(weapon.Type.ToString());
                    loot.PlayAnimation();
                    await Task.Delay(loot.AsyncDelay);
                }
            }

            if (_gameModel.IncomeLoot.Soft > 0)
            {
                var loot = Pool.Pop<LootItemUI>(_lootContainer);
                _lootItems.Add(loot);
                loot.SetSprite("Soft");
                loot.SetText($"{_gameModel.IncomeLoot.Soft}");
                loot.PlayAnimation();
                await Task.Delay(loot.AsyncDelay);
            }

            if (_gameModel.IncomeLoot.Hard > 0)
            {
                var loot = Pool.Pop<LootItemUI>(_lootContainer);
                _lootItems.Add(loot);
                loot.SetSprite("Hard");
                loot.SetText($"{_gameModel.IncomeLoot.Hard}");
                loot.PlayAnimation();
                await Task.Delay(loot.AsyncDelay);
            }
            
            if (_gameModel.IncomeLoot.Experience > 0)
            {
                var loot = Pool.Pop<LootItemUI>(_lootContainer);
                _lootItems.Add(loot);
                loot.SetSprite("Experience");
                loot.SetText($"{_gameModel.IncomeLoot.Experience}");
                loot.PlayAnimation();
                await Task.Delay(loot.AsyncDelay);
            }

            _nextButton.Activate();
        }

        private void ClearLootItems()
        {
            foreach (var lootItem in _lootItems)
            {
                lootItem.PushToPool();
            }
            _lootItems.Clear();
        }

        public override void Close()
        {
            ClearLootItems();

            base.Close();
        }

        private void OnNextButtonPressed()
        {
            _gameModel.IncomeLoot.ConfirmAll();
            if (_gameModel.CurrentData.Region != GameBalance.Instance.TutorialLevelID)
            {
                _ad.ShowAd(AdType.Interstitial, AdVideoType.Interstitial);
            }
            if (_isWin)
            {
                _gameModel.NextLevel();   
            }
            else
            {
                _gameModel.RestartLevel();
            }
            Close();
        }
    }
}