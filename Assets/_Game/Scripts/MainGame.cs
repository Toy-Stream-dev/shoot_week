using _Game.Scripts.Model;
using DG.Tweening;
using GeneralTools;
using GeneralTools.Model;
using GeneralTools.UI;
using UnityEngine;

namespace _Game.Scripts
{
	public class MainGame : BaseBehaviour
	{
		[SerializeField] private Transform _unitsContainer;
		[SerializeField] private Transform _projectilesContainer;
		[SerializeField] private Transform _dropContainer;
		[SerializeField] private Transform _worldSpaceCanvas;
		
		private LoaderModel _loader;

		public static Transform Root { get; private set; }
		public static Transform UnitsContainer { get; private set; }
		public static Transform ProjectilesContainer { get; private set; }
		public static Transform WorldSpaceCanvas { get; private set; }
		public static Transform DropContainer { get; private set; }
		
		public static bool Active { get; private set; } = true;

		private void Start()
		{
#if UNITY_EDITOR
			QualitySettings.vSyncCount = 0;
#else
			QualitySettings.vSyncCount = 0;
			Application.targetFrameRate = 60;
#endif

			Root = transform;
			UnitsContainer = _unitsContainer;
			ProjectilesContainer = _projectilesContainer;
			WorldSpaceCanvas = _worldSpaceCanvas;
			DropContainer = _dropContainer;
			
			_loader = Models.Add<LoaderModel>();
			_loader.LoadedEvent += OnLoaded;
			_loader.Start();
		}

		private void OnLoaded()
		{
			Models.Get<GameModel>().OnLoaded();
		}

		private void Update()
		{
			if (!Active) return;

			var deltaTime = Time.deltaTime;

			if (_loader.State != LoaderState.Finished)
			{
				_loader.Update(deltaTime);
				return;
			}

			DOTween.ManualUpdate(Time.deltaTime, Time.unscaledDeltaTime);

			Models.Update(deltaTime);
			GameUI.UpdateMe(deltaTime);
			Cheats.Update();
		}

		private void LateUpdate()
		{
			if (_loader.State == LoaderState.Finished)
			{
				GameUI.LateUpdateMe();
			}
		}
	}
}