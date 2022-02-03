using System;
using System.Collections.Generic;
using _Game.Scripts.Ad;
using _Game.Scripts.Analytics;
using _Game.Scripts.Balance;
using _Game.Scripts.Model.Drop;
using _Game.Scripts.Model.Items;
using _Game.Scripts.Model.Unit;
using _Game.Scripts.Saves;
using _Game.Scripts.Tutorial;
using _Game.Scripts.UI.Windows;
using GeneralTools;
using GeneralTools.Localization;
using GeneralTools.Model;
using GeneralTools.Pooling;
using GeneralTools.UI;

namespace _Game.Scripts.Model
{
	public enum LoaderState
	{
		None,
		Started,
		InitializingComponents,
		LoadingSave,
		Finishing,
		Finished
	}

	public class LoaderModel : BaseModel
	{
		private readonly Dictionary<LoaderState, float> _stateWeights = new Dictionary<LoaderState, float>()
		{
			{LoaderState.Started, 0.05f},
			{LoaderState.InitializingComponents, 0.15f},
			{LoaderState.LoadingSave, 0.3f},
			{LoaderState.Finishing, 0.5f}
		};

		public event Action LoadedEvent;

		private LoadingWindow _loadingWindow;
		private List<Action> _loadingActions = new List<Action>();
		private float _loadingActionWeight;
		private float Progress { get; set; }
		
		public LoaderState State { get; private set; }

		public override BaseModel Start()
		{
			if (State != LoaderState.None) return this;
			StartLoading();
			return base.Start();
		}

		private void StartLoading()
		{
			Progress = _stateWeights[LoaderState.Started];

			Prefabs.Init();
			GameSettings.Load();
			
			OpenLoading();

			State = LoaderState.Started;
		}

		private void OpenLoading()
		{
			_loadingWindow = GameUI.GetBeforeInit<LoadingWindow>();
			_loadingWindow.Open(Progress);
		}

		private void OnLoadingStateCompleted()
		{
			State = ++State;
			switch (State)
			{
				case LoaderState.InitializingComponents:
					StartInitializingComponents();
					break;

				case LoaderState.LoadingSave:
					StartLoadingSave();
					break;

				case LoaderState.Finishing:
					StartFinishing();
					break;

				case LoaderState.Finished:
					OnLoaded();
					break;
			}

			_loadingActionWeight = _stateWeights.ContainsKey(State)
				                       ? _loadingActions.Count == 0 ? _stateWeights[State] :
				                                                      _stateWeights[State] / _loadingActions.Count
				                       : 0f;
		}

		public override void Update(float deltaTime)
		{
			switch (State)
			{
				case LoaderState.None:
				case LoaderState.Finished:
					return;
			}

			if (_loadingActions.Count == 0)
			{
				OnLoadingStateCompleted();
				return;
			}

			_loadingActions[0]?.Invoke();
			_loadingActions.RemoveAt(0);

			Progress += _loadingActionWeight;
			_loadingWindow.UpdateProgress(Progress);
		}

		private void StartInitializingComponents()
		{
			_loadingActions = new List<Action>
			{
				GameBalance.Init,
				CreateModels,
				InitPool,
				InitLocalization
			};
		}

		private void InitLocalization()
		{
			Localization.LanguageChangedEvent += GameUI.UpdateLocalization;
		}

		private void CreateModels()
		{
			Models.Add<GameModel>();
			Models.Add<MathModel>();
			Models.Add<MapModel>();
			Models.Add<InputModel>();
			Models.Add<AdModel>();
			Models.Add<AnalyticsModel>();
			Models.Add<GameEffectModel>();
			Models.Add<ItemsModel>();
			Models.Add<DropContainer>();
			Models.Add<TutorialModel>();
		}

		private void InitPool()
		{
			Pool.Init(null);
		}

		private void StartLoadingSave()
		{
			_loadingActions = new List<Action>
			{
				LoadSave
			};
		}

		private void StartFinishing()
		{
			_loadingActions = new List<Action>
			{
				GameUI.Init,
				GameSave.Save,
				Models.Start,
			};
		}

		private void LoadSave()
		{
			if (!GameSave.Exists() || DevFlags.RESET)
			{
				GameSave.DeleteSave();
				return;
			}

			var snapshot = GameSave.Load();
			Models.Get<GameModel>().CopyFrom(snapshot.Game);
			Models.Get<ItemsModel>().CopyFrom(snapshot.Items);
		}
		
		public void EmulateLoading()
		{
			//State = LoaderState.EmulateLoading;
			_loadingWindow.EmulateLoading(1f);
			//_loadingWindow.OnCompleteEmulate += OnCompleteEmulate;
		}

		private void OnLoaded()
		{
			_loadingWindow.Close();
			LoadedEvent?.Invoke();
		}

	}
}