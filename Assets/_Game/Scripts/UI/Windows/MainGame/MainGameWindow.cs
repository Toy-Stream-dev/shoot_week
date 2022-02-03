using System.Linq;
using _Game.Scripts.Balance;
using _Game.Scripts.Enums;
using _Game.Scripts.Model;
using _Game.Scripts.Model.Items;
using _Game.Scripts.Model.Unit;
using _Game.Scripts.UI.HUD;
using _Game.Scripts.UI.Progress;
using GeneralTools.Model;
using GeneralTools.Tools;
using GeneralTools.UI;
using Plugins.GeneralTools.Scripts.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Game.Scripts.UI.Windows.MainGame
{
    public class MainGameWindow : BaseWindow
    {
        [SerializeField] private TextMeshProUGUI _levelInfo;
        [SerializeField] private TextMeshProUGUI _soft;
        [SerializeField] private TextParamUI _softParamUI;
        [SerializeField] private TextMeshProUGUI _hard;
        [SerializeField] private TextParamUI _hardParamUI;
        [SerializeField] private MainGameItem[] _hudItems;
        [SerializeField] private BaseButton _pauseButton;
        [SerializeField] private MaskUI _maskUI;
        [SerializeField] private TextMeshProUGUI _maskInfo;
        [SerializeField] private Animation _maskInfoAnimation;
        [SerializeField] private GameObject _bossHp;
        [SerializeField] private TextMeshProUGUI _bossName;
        [SerializeField] private Slider _bossHpProgress;

        private GameModel _game;
        private GameBalance.StageConfig _stageConfig;

        public override void Init()
        {
            _maskUI.Deactivate();
            _maskUI.MaskEnd += OnMaskEnd;
            _pauseButton.SetCallback(OnPressedPause);
            _game = Models.Get<GameModel>();
            _softParamUI.SetParam(_game.CurrentData.GetParam(GameParamType.Soft));
            _hardParamUI.SetParam(_game.CurrentData.GetParam(GameParamType.Hard));
			
            foreach (var item in _hudItems)
            {
                item.Init();
                item.OnEquipItem += OnEquipWeapon;
            }
			
            base.Init();
        }

        public MainGameItem GetGameItem(ItemData itemData)
        {
            return _hudItems.FirstOrDefault(item => item.ItemData == itemData);
        }

        public void OnEquipWeapon(ItemData itemData)
        {
            foreach (var item in _hudItems)
            {
                if (item.ItemData != itemData)
                {
                    item.SetColor(false);
                }
                else
                {
                    item.SetColor(true);
                }
            }
        }
        

        public void SetMask(GameBalance.MaskConfig mask)
        {
            ShowInfo(mask.Text);
            _maskUI.SetMask(mask);
            _maskUI.Activate();
        }

        public void ShowInfo(string text)
        {
            _maskInfo.text = text;
            _maskInfoAnimation.Play();
        }

        public void OnMaskEnd()
        {
            if (!_maskUI.IsActivate()) return;
            _maskUI.KillTween();
            _maskUI.Deactivate();
        }

        private void OnPressedPause()
        {
            Cheats.PauseGame();
            GameUI.Get<PauseWindow>().Open();
        }

        public void ShowItems()
        {
            // _soft.text = _game.CurrentData.GetParam(GameParamType.Soft).Value.ToString();
            // _hard.text = _game.CurrentData.GetParam(GameParamType.Hard).Value.ToString();
            
            for (int i = 0; i < _hudItems.Length; i++)
            {
                _hudItems[i].Show(i < _game.ChosenWeapons.Count ? _game.ChosenWeapons[i] : null);
            }
        }

        public void InitBoss(UnitModel boss)
        {
            boss.OnHpChange += SetBossHP;
            boss.OnDie += OnBossDie;
            _bossHp.Activate();
            _bossName.text = boss.EnemyConfig.Name;
        }

        public void OnBossDie(UnitModel boss)
        {
            if (boss != null)
            {
                boss.OnDie -= OnBossDie;
                boss.OnHpChange -= SetBossHP;   
            }
            _bossHpProgress.value = 1;
            _bossHp.Deactivate();
        }

        public void SetBossHP(float value)
        {
            _bossHpProgress.value = value;
        }

        public void RedrawLevelInfo()
        {
            var indexes = _stageConfig.GetStage(_game.CurrentData.Region);
            
            if (indexes.Item1 < 0)
            {
                _levelInfo.text = $"AREA {_stageConfig.Id}-{_stageConfig.Regions.Count + _game.FakeLevelID}";
            }
            else
            {
                _levelInfo.text = $"AREA {_stageConfig.Id}-{indexes.Item1}";
            }
        }

        public void RedrawItems()
        {
            foreach (var item in _hudItems)
            {
                item.Redraw();
            }
        }

        public void UpdateStageInfo()
        {
            var region = _game.CurrentData.Region;
            _stageConfig = GameBalance.Instance.StageConfigs.FirstOrDefault(stage => stage.HasRegion(region)) ??
                           GameBalance.Instance.StageConfigs.Last();
            RedrawLevelInfo();
        }
    }
}