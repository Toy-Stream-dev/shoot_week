using System.Collections.Generic;
using System.Linq;
using _Game.Scripts.Balance;
using _Game.Scripts.Enums;
using _Game.Scripts.Model;
using _Game.Scripts.UI.Progress;
using _Game.Scripts.UI.Windows.Inventory;
using _Game.Scripts.UI.Windows.MainGame;
using GeneralTools.Model;
using GeneralTools.UI;
using Plugins.GeneralTools.Scripts.SafeArea;
using Plugins.GeneralTools.Scripts.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Game.Scripts.UI.HUD
{
	public class HUD : BaseWindow
	{
		[SerializeField] private BaseButton _inventoryButton;
		[SerializeField] private BaseButton _mapButton;
		[SerializeField] private BaseButton _playButton;
		[SerializeField] private TextMeshProUGUI _level;
		[SerializeField] private TextMeshProUGUI _exp;
		[SerializeField] private TextMeshProUGUI _soft;
		[SerializeField] private TextMeshProUGUI _hard;
		[SerializeField] private TextMeshProUGUI _map;
		[SerializeField] private TextMeshProUGUI _stage;
		[SerializeField] private Image _stageImage;
		[SerializeField] private SliderProgressUI _sliderProgressUI;
		[SerializeField] private List<SafeArea> _safeAreas = new List<SafeArea>();
		[SerializeField] private List<GamePlayElementUI> GamePlayElements = new List<GamePlayElementUI>();
		[SerializeField] private Transform _mainCanvas;
		[SerializeField] private TextMeshProUGUI _bossText;

		[SerializeField] private Image _mapImage;

		public TextMeshProUGUI BossText => _bossText;
		private MainGameWindow _mainGame;
		private GameModel _game;
		private GameBalance.StageConfig _stageConfig;
        
		public override void Init()
		{
			//GamePlayElements = _mainCanvas.GetComponentsInChildren<GamePlayElementUI>().ToList();
			_inventoryButton.Callback = () => GameUI.Get<InventoryWindow>().Open();
			_mapButton.SetCallback(OnPressMapButton);
			_playButton.SetCallback(OnPressedPlay);
			_mainGame = GameUI.Get<MainGameWindow>();
			_game = Models.Get<GameModel>();
			_sliderProgressUI.SetProgress(_game.CurrentData.GetProgress(GameParamType.PlayerLevel));
			UpdateOffset();
			UpdateStageInfo();
            
			base.Init();
		}

		public void UpdateOffset()
		{
			foreach (var safeArea in _safeAreas)
			{
				safeArea.UpdateOffset();
			}
		}

		public void UpdateStageInfo()
		{
			var region = _game.CurrentData.Region;
			_stageConfig = GameBalance.Instance.StageConfigs.FirstOrDefault(stage => stage.HasRegion(region)) ??
			                   GameBalance.Instance.StageConfigs.Last();
			_stageImage.sprite = _stageConfig.Image;
			Redraw();
		}

		public override BaseUI Open()
		{
			Redraw();
			return base.Open();
		}

		private void OnPressMapButton()
		{
			GameUI.Get<InventoryWindow>().Close();
		}

		public void OnPressedPlay()
		{
			_mainGame.ShowItems();
			_game.OnPressedStart();
			_mainGame.Open();

			Close();
		}

		public void Redraw()
		{
			_soft.text = _game.CurrentData.GetParam(GameParamType.Soft).Value.ToString();
			_hard.text = _game.CurrentData.GetParam(GameParamType.Hard).Value.ToString();
			_level.text = _game.CurrentData.GetParam(GameParamType.PlayerLevel).Level.ToString();

			var exp = _game.CurrentData.GetProgress(GameParamType.PlayerLevel);
			_exp.text = $"{exp.CurrentValue}/{exp.Target}";

			//_mapImage.sprite = $"map_{_game.CurrentData.GetParam(GameParamType.MapId).Value}".GetSprite();

			_map.text = $"{_stageConfig.Id}. {_stageConfig.MapText}";
			var indexes = _stageConfig.GetStage(_game.CurrentData.Region);
			if (indexes.Item1 < 0)
			{
				_stage.text = $"Stage: {_stageConfig.Regions.Count + _game.FakeLevelID}";
			}
			else
			{
				_stage.text = $"Stage: {indexes.Item1}";	
			}
		}
		
		public BaseUIBehaviour GetGameplayElement(GamePlayElement element)
		{
			return GamePlayElements.Find(e => e.Element == element);
		}

		public void AddGameplayElement(GamePlayElementUI gamePlayElementUI)
		{
			GamePlayElements.Add(gamePlayElementUI);
		}
	}
}