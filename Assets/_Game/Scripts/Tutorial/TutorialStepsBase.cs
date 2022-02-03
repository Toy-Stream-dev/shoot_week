using System;
using System.Collections.Generic;
using System.Linq;
using _Game.Scripts.Model;
using _Game.Scripts.UI.HUD;
using _Idle.Scripts.Tutorial;
using _Idle.Scripts.Tutorial.UI;
using GeneralTools.Model;
using GeneralTools.Tools;
using GeneralTools.UI;
using Plugins.GeneralTools.Scripts.UI;
using UnityEngine;

namespace _Game.Scripts.Tutorial
{
	public abstract class TutorialStepsBase
	{
		protected readonly GameModel Game;
		protected readonly MapModel Map;
		protected readonly HUD Hud;
		protected readonly TutorialWindow TutorialWindow;
		
		private List<Func<(TutorialNextStepCondition,object)>> _steps;

		public int CurrentTutorialId { get; protected set; }
		public int CurrentStepIndex { get; protected set; }
		public bool BlockedButtons { get; protected set; }
		
		public TutorialNextStepCondition NextStepCondition { get; protected set; }
		public object NextStepConditionParam { get; protected set; }
		
		public bool IsLastStep => CurrentStepIndex == _steps.Count - 1;

		public int LastStepStartFrame { get; private set; }
		public bool LastStepIsJustStarted => LastStepStartFrame == Time.frameCount;

		protected abstract string TokenPrefix { get; }

		protected TutorialStepsBase()
		{
			Game = Models.Get<GameModel>();
			Map = Models.Get<MapModel>();

			Hud = GameUI.Get<HUD>();
			TutorialWindow = GameUI.Get<TutorialWindow>();
		}

		protected void RegisterTutorialSteps(params Func<(TutorialNextStepCondition, object)>[] steps)
		{
			_steps = steps.ToList();
		}

		public void StartStep(int stepIndex, int tutorialId = -1, bool blockedButtons = true)
		{
			if (tutorialId > 0) CurrentTutorialId = tutorialId;
			BlockedButtons = blockedButtons;
			
			LastStepStartFrame = Time.frameCount;

			if (TutorialWindow.IsClosed)
			{
				TutorialWindow.Open();
			}
			else
			{
				TutorialWindow.HideAll();
			}
			
			CurrentStepIndex = stepIndex;
			(NextStepCondition, NextStepConditionParam) = _steps[stepIndex].Invoke();

			if (NextStepCondition == TutorialNextStepCondition.GoToNextStep)
			{
				StartStep(CurrentStepIndex + 1);
				return;
			}
			
			//AppEventsProvider.TriggerEvent(GameEvents.TutorialStepFinished, CurrentTutorialId, stepIndex);
			Debug.Log($"tutorial step {stepIndex}");
		}

		public virtual void FinishTutorial()
		{
			SceneInteraction.Flags.Clear(InteractionFlags.IgnoreSceneDrag);
			SceneInteraction.Flags.Clear(InteractionFlags.IgnoreSceneTaps);

			CurrentStepIndex = -1;
			CurrentTutorialId = -1;
			
			TutorialWindow.Close();
		}

		protected void CloseAllWindows()
		{
			var windows = GameUI.Get<BaseWindow>().GetOpenedWindows();
			if (windows.Count == 0) return;

			var list = new List<BaseWindow>();
			
			foreach (var window in windows)
			{
				if (window == Hud) continue;
				list.Add(window);
			}

			foreach (var window in list)
			{
				window.Close();
			}
		}
	}
}