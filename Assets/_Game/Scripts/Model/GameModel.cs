using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using _Game.Scripts.Ad;
using _Game.Scripts.Balance;
using _Game.Scripts.Enums;
using _Game.Scripts.Model.Base;
using _Game.Scripts.Model.Drop;
using _Game.Scripts.Model.Items;
using _Game.Scripts.Model.Numbers;
using _Game.Scripts.Model.Unit;
using _Game.Scripts.Saves;
using _Game.Scripts.Tutorial;
using _Game.Scripts.UI;
using _Game.Scripts.UI.HUD;
using _Game.Scripts.UI.Windows;
using _Game.Scripts.UI.Windows.Inventory;
using _Game.Scripts.UI.Windows.MainGame;
using _Game.Scripts.UI.Windows.Mask_window;
using _Game.Scripts.View;
using DG.Tweening;
using GeneralTools.Model;
using GeneralTools.Pooling;
using GeneralTools.Tools;
using GeneralTools.UI;
using UnityEngine;
using UnityEngine.Scripting;

namespace _Game.Scripts.Model
{
	public class GameModel : GameModelBase, IModelWithParam, ISerializationCallbackReceiver
	{
		private UnitContainer _playerContainer; 
		private UnitContainer _enemiesContainer;
		
		public List<ItemData> ChosenWeapons = new List<ItemData>();
		public List<ProjectileView> Projectiles { get; private set; } = new List<ProjectileView>();
		public UnitContainer EnemyContainer => _enemiesContainer;
		//[SerializeField] private List<ItemData> _saveWeapons = new List<ItemData>();

		// public List<ItemData> ChosenWeapons => _chosenWeapons;

		[SerializeField] private int _fakeLevelID;
		public int FakeLevelID => _fakeLevelID;

		public UnitModel Player { get; private set; }
		public UnitModel StorePlayer { get; private set; }
		public bool LevelClear { get; private set; }
		public event Action LevelCleared;
		
		private GameParam _totalPlayTime;
		private MapModel _map;
		private BattleSettings _battle;
		private InputModel _inputModel;
		private TutorialModel _tutorial;
		private GameProgress _offerRespawnDelay;
		public IncomeLoot IncomeLoot;

		private DateTime _firstSessionStart;
		private LoseWindow _loseWindow;

		[SerializeField] private int _tutorialId;
		[SerializeField] private int _tutorialStep;
		[SerializeField] private int _last24SessionDay;
		[SerializeField] private string _firstSessionStartStr;

		public override BaseModel Init()
		{
			base.Init();
			IncomeLoot = new IncomeLoot(this);
			//FakeLevelID = 1;
			_playerContainer = new UnitContainer();
			_enemiesContainer = new UnitContainer();

			CreateParam(GameParamType.SessionN);
			InitGameData();
			
			_totalPlayTime = CreateParam(GameParamType.TotalPlayTime, 0, false);

			return this;
		}

		public override BaseModel Start()
		{
			base.Start();

			Flags.ChangedEvent += OnFlagChanged;
			
			GameUI.Get<MainGameWindow>().UpdateStageInfo();
			GameUI.Get<MainGameWindow>().RedrawLevelInfo();
			GameUI.Get<IndicatorWindow>().Open();
			_loseWindow = GameUI.Get<LoseWindow>();
			_map = Models.Get<MapModel>();
			_tutorial = Models.Get<TutorialModel>();
			_battle = BattleSettings.Instance;
			_inputModel = Models.Get<InputModel>();
			_playerContainer.Start();
			_enemiesContainer.Start();
			GameSounds.Instance.Init();

			InitPlayer();
			InitPool();
			CreateProgress();
			
			// if (_saveWeapons.Count > 0)
			// {
			// 	foreach (var saveWeapons in _saveWeapons)
			// 	{
			// 		ChooseWeapon(saveWeapons, false);
			// 		GameUI.Get<InventoryWindow>().AddWeapon(saveWeapons);
			// 	}
			// 	if (ChosenWeapons.Count == 0)
			// 	{
			// 		StorePlayer.SetEmptyHand();
			// 	}
			// 	else
			// 	{
			// 		StorePlayer.EquipWeapon(ChosenWeapons.First());	
			// 	}
			// }
			
			CheckStartedSession();

			return this;
		}

		private void CreateProgress()
		{
			_offerRespawnDelay = new GameProgress(GameParamType.Timer, GameBalance.Instance.OfferRespawnDelay, false);
			_offerRespawnDelay.Pause();
		}

		private void InitPlayer()
		{
			Player = _playerContainer.SpawnPlayer();
			_inputModel.SetPlayer(Player);
			StorePlayer = _playerContainer.SpawnStorePlayer();
			GameCamera.Instance.Follow(Player.View.CameraFollow, false);
		}

		public void RespawnPlayer()
		{
			Player.OnMaskEnded();
			GameUI.Get<MainGameWindow>().OnMaskEnd();
			var deathPoint = Player.DeathPoint;
			_playerContainer.ClearAll();
			if (Player.View == null)
			{
				_playerContainer.SpawnView(Player);
				EquipItems();
				GameCamera.Instance.Follow(Player.View.CameraFollow, false);
				_inputModel.SetPlayer(Player);
			}
			Player.SetPosition(deathPoint);
			var mask = new GameBalance.MaskConfig(GameBalance.Instance.Masks.FirstOrDefault(mask => mask.Type == GameBalance.MaskType.Immortal));
			mask.Duration = 5;
			Player.EquipMask(mask);
			ReloadAllWeapon();
			EquipItems();
			InputOn();
		}

		private void InitPool()
		{
			Pool.Spawn<ProjectileView>(20, item => item.ProjectileType == UnitType.Player);
			Pool.Spawn<ProjectileView>(50, item => item.ProjectileType == UnitType.EnemyBase);
		}
		
		private void OnFlagChanged(GameFlag flag, bool active)
		{
		}
		
		public override void Update(float deltaTime)
		{
			_playerContainer.Update(deltaTime);
			_enemiesContainer.Update(deltaTime);
			_offerRespawnDelay.Change(deltaTime);
			if (_loseWindow.IsOpened)
			{
				_loseWindow.UpdateMe(deltaTime);
			}
			
			GameSounds.Instance.UpdateMe(deltaTime);

			for (int i = 0; i < Projectiles.Count; i++)
			{
				Projectiles[i].UpdateMe(deltaTime);
			}

			IncPlayTime(deltaTime);
			base.Update(deltaTime);
		}

		public void OnLoaded()
		{
			foreach (var item in Models.Get<ItemsModel>().Items)
			{
				if (item.Chosen && !ChosenWeapon(item))
				{
					ChooseWeapon(item);
					GameUI.Get<InventoryWindow>().AddWeapon(item);
				}
			}
			StartLevel();
			if (CurrentData.Region == GameBalance.Instance.TutorialLevelID)
			{
				GameSounds.Instance.PlayMusic(GameSoundType.MainTheme);
			}
			else
			{
				GameSounds.Instance.PlayMusic(GameSoundType.Hud);
			}
			Models.Get<AdModel>().ShowAd(AdType.Banner, AdVideoType.Banner);
		}

		public void OnPressedStart()
		{
			EquipItems();
			if (CurrentData.Region >= GameBalance.Instance.ShowMaskLevel)
			{
				GameUI.Get<ChooseMaskWindow>().Open();
			}
			else
			{
				InputOn();
			}

			var enemies = _enemiesContainer.GetAllAlive();
			foreach (var enemy in enemies)
			{
				enemy.InitIndicator();
			}
			
			GameSounds.Instance.PlayMusic(GameSoundType.MainTheme);
			_offerRespawnDelay.Reset();
			AppEventsProvider.TriggerEvent(GameEvents.LevelStart, CurrentData.Region);
		}
		
		private void StartLevel()
		{
			ReloadAllWeapon();
			GameUI.Get<HUD>().Open();
			LevelClear = false;
			LoadMapViews();
			if (Player.View == null)
			{
				_playerContainer.SpawnView(Player);
				EquipItems();
				GameCamera.Instance.Follow(Player.View.CameraFollow, false);
				_inputModel.SetPlayer(Player);
			}
			Player.SetPosition(_map.View.PlayerPosition);
			if (_map.View.Boss != null)
			{
				GameUI.Get<MainGameWindow>().InitBoss(_map.View.Boss.Model);
			}
			if (CurrentData.Region == GameBalance.Instance.TutorialLevelID)
			{
				GameUI.Get<HUD>().OnPressedPlay();
			}

			if (!Flags.Has(GameFlag.ShotTutorial))
			{
				_tutorial.StartTutorial(TutorialType.Shot);
			}

			GameUI.Get<HUD>().BossText.SetActive(_map.View.Boss != null);
		}
		
		private void LoadMapViews()
		{
			var region = CurrentData.Region;
			if (region >= GameBalance.Instance.MaxLevelID)
			{
				region = GameBalance.Instance.MaxLevelID;
			}
			_map.InitView(region);
			foreach (var dropView in _map.View.DropViews)
			{
				var dropModel = new DropModel();
				dropModel.SetView(dropView);
				dropModel.Start();
			}
			
			_enemiesContainer.InitEnemies();
			StorePlayer.SetPosition(_map.View.StorePlayerPosition);
		}

		private void IncreaseMapId()
		{
			//FakeLevelID++;
			if (CurrentData.Region >= GameBalance.Instance.MaxLevelID)
			{
				_fakeLevelID++;
			}
			CurrentData.IncRegion();
			GameUI.Get<HUD>().UpdateStageInfo();
			GameUI.Get<MainGameWindow>().UpdateStageInfo();
			GameUI.Get<MainGameWindow>().RedrawLevelInfo();
		}

		public void CompleteLevel(bool successful)
		{
			if (_offerRespawnDelay.IsCompleted && !successful)
			{
				_offerRespawnDelay.Reset();
				_offerRespawnDelay.Pause();
				_inputModel.SetState(InputState.Pause);

				GameUI.Get<LoseWindow>().Open();
			}
			else
			{
				Models.Get<DropContainer>().KillTweenAll();
				if (successful)
				{
					var winWindow = GameUI.Get<WinWindow>();
					if (winWindow.IsClosed)
					{
						winWindow.Open(true);
						AppEventsProvider.TriggerEvent(GameEvents.LevelComplete, CurrentData.Region);
					}
				}
				else
				{
					var winWindow = GameUI.Get<WinWindow>();
					if (winWindow.IsClosed)
					{
						winWindow.Open(false);
						AppEventsProvider.TriggerEvent(GameEvents.LevelFail, CurrentData.Region);
					}
				}
				_inputModel.SetState(InputState.Pause);
				_offerRespawnDelay.Pause();
			}
		}

		public void RestartLevel()
		{
			ClearLevel();
			StartLevel();
			GameSave.Save();
		}

		public void NextLevel()
		{
			ClearLevel();
			IncreaseMapId();
			StartLevel();
			if (Flags.Has(GameFlag.ShotTutorial) && !Flags.Has(GameFlag.InventoryTutorial))
			{
				_tutorial.StartTutorial(TutorialType.Inventory);
			}
			GameSave.Save();
		}

		private void ClearLevel()
		{
			foreach (var dropedItem in Player.DropedItems)
			{
				var weapon = Models.Get<ItemsModel>().Items.Find(item => item == dropedItem);
				if (weapon != null)
				{
					ChooseWeapon(weapon);
				}
			}
			Player.OnMaskEnded();
			GameUI.Get<MainGameWindow>().OnMaskEnd();
			GameUI.Get<MainGameWindow>().OnBossDie(null);
			IncomeLoot.ClearAll();
			GameUI.Get<MainGameWindow>().Close();
			Models.Get<DropContainer>().DestroyAll();
			_map.Destroy();
			_playerContainer.ClearAll();
			_enemiesContainer.ClearAll();
			var healthBars = MainGame.WorldSpaceCanvas.GetComponentsInChildren<ProgressBarView>().ToList();
			foreach (var healthBar in healthBars)
			{
				healthBar.PushToPool();
			}
			GameSounds.Instance.PlayMusic(GameSoundType.Hud);
			GarbageCollector.CollectIncremental();
		}

		public void SetLevelClear()
		{
			LevelClear = true;
			LevelCleared?.Invoke();
		}

		public void IncMapId()
		{
			CurrentData.IncRegion();

			_map.Destroy();
			_map.InitView(CurrentData.Region);
			_map.OnLoad();
			
			_enemiesContainer.InitEnemies();

			GameSave.Save();
		}

		private void ClearChosenItems()
		{
			ChosenWeapons.Clear();
		}
		
		public void EquipItems()
		{
			if (ChosenWeapons.Count == 0) return;
			Player.EquipWeapon(ChosenWeapons);
			StorePlayer.EquipWeapon(ChosenWeapons.First());
		}

		public void InputOn()
		{
			_inputModel.SetState(InputState.Play);
		}

		public void InputOff()
		{
			_inputModel.SetState(InputState.Pause);
		}

		public UnitModel GetEnemy()
		{
			return _enemiesContainer.GetNearestEnemy(Player.View.transform.position);
		}

		public void CreateProjectile(int count, UnitType type)
		{
			Pool.Spawn<ProjectileView>(count, projectile => projectile.ProjectileType == type);
		}
		
		public ProjectileView SpawnProjectile(ItemType type, Vector3 spawnPos, Quaternion rotation, float damage, UnitModel owner)
		{
			var config = _battle.Projectiles.FirstOrDefault(p => p.Type == type);
			if (config == null) return null;
			var projectile = Pool.Pop<ProjectileView>(MainGame.ProjectilesContainer, true, item => item.ProjectileType == owner.Type);
			projectile.Init(config, spawnPos, rotation, damage, owner);
			Projectiles.Add(projectile);

			return projectile;
		}

		public void RemoveProjectile(ProjectileView projectile)
		{
			Projectiles.Remove(projectile);
			projectile.PushToPool();
		}

		public void ReloadAllWeapon()
		{
			foreach (var weapon in ChosenWeapons)
			{
				weapon.ResetAmmunition();
			}
			//GameSave.Save();
		}

		public void ChooseWeapon(ItemData item)
		{
			ChosenWeapons.Add(item);
			item.SetChosen(true);
			if (ChosenWeapons.Count == 0)
			{
				StorePlayer.SetEmptyHand();
			}
			else
			{
				StorePlayer.EquipWeapon(ChosenWeapons.First());	
			}
			GameSave.Save();
		}
		
		public void RemoveWeapon(ItemData item)
		{
			ChosenWeapons.Remove(item);
			item.SetChosen(false);
			GameUI.Get<MainGameWindow>().ShowItems();
			if (ChosenWeapons.Count == 0)
			{
				StorePlayer.SetEmptyHand();
			}
			else
			{
				StorePlayer.EquipWeapon(ChosenWeapons.First());	
			}
			GameSave.Save();
		}
		
		public bool ChosenWeapon(ItemData item)
		{
			return ChosenWeapons.Contains(item);
		}

		private void IncPlayTime(float deltaTime)
		{
			var prevMinute = (int)_totalPlayTime.Value / 60;
			_totalPlayTime.Change(deltaTime);
			var minutes = (int)_totalPlayTime.Value / 60;

			if (prevMinute != minutes && minutes <= 60)
			{
				AppEventsProvider.TriggerEvent(GameEvents.Timer, minutes, GetParam(GameParamType.SessionN).IntValue);
			}
		}
		
		private void CheckStartedSession()
		{
			var sessionN = GetParam(GameParamType.SessionN);
			if (sessionN.IntValue == 0)
			{
				_firstSessionStart = DateTime.Now;
				//AppEventsProvider.TriggerEvent(GameEvents.FirstSession);
			}

			sessionN.Change(1);

			if ((int)_totalPlayTime.Value / 60 < 60) return;
			var totalDaysFromStart = (int)(DateTime.Now - _firstSessionStart).TotalDays;
			if (totalDaysFromStart < _last24SessionDay) return;

			_last24SessionDay = totalDaysFromStart;

			AppEventsProvider.TriggerEvent(GameEvents.Timer,
			                               totalDaysFromStart,
			                               (int)_totalPlayTime.Value / 60,
			                               GetParam(GameParamType.SessionN).IntValue);
		}
		
		public bool IsEnough(GameParamType param, BigNumber needed)
		{
			return CurrentData.GetParam(param).Value >= needed.Value;
		}
		
		public void AddMoney(BigNumber amount)
		{
			var current = CurrentData.GetParam(GameParamType.Soft);
			var value = amount.Value;
			
			if (value <= 0) return;
			current.Change(value);
		}

		public void SpendMoney(BigNumber amount)
		{
			var current = CurrentData.GetParam(GameParamType.Soft);
			var value = amount.Value;

			if (current.Value - value < 0)
			{
				Debug.LogError($"Can`t spend {value} money");
				return;
			}
			current.Change(-value);
		}
		
		public void CopyFrom(GameModel source)
		{
			base.CopyFrom(source);

			//ChosenWeapons = source.ChosenWeapons;
			_fakeLevelID = source._fakeLevelID;
			_firstSessionStart = source._firstSessionStart;
			_tutorialId = source._tutorialId;
			_tutorialStep = source._tutorialStep;
		}

		public override void OnBeforeSerialize()
		{
			_firstSessionStartStr = _firstSessionStart.ToString(CultureInfo.InvariantCulture);
			base.OnBeforeSerialize();
		}

		public override void OnAfterDeserialize()
		{
			_firstSessionStart = DateTime.Parse(_firstSessionStartStr, CultureInfo.InvariantCulture);
			base.OnAfterDeserialize();
		}
	}
}