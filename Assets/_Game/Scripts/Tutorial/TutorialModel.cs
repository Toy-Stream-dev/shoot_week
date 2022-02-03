using System;
using System.Collections.Generic;
using _Game.Scripts.Model;
using _Game.Scripts.Tutorial.UI;
using GeneralTools.Model;
using GeneralTools.Tools;
using GeneralTools.UI;
using Plugins.GeneralTools.Scripts.UI;
using UnityEngine;

namespace _Game.Scripts.Tutorial
{
	public enum TutorialType
	{
		None,
		Shot,
		Inventory,
	}

	public class TutorialModel : BaseModel
	{
		private static readonly Dictionary<TutorialType, Type> TutorialSteps = new Dictionary<TutorialType, Type>()
		{
			{TutorialType.Shot, typeof(IntroTutorialSteps)},
			{TutorialType.Inventory, typeof(UpgradeTutorialSteps)}
		};

		private const float TAP_TIME = 0.5f;

		private GameModel _game;
		private TutorialWindow _tutorial;
		private float _tapStartTime;

		public TutorialStepsBase Steps { get; private set; }
		public bool IsActive { get; private set; }
		public event Action TutorEnd;

		public override BaseModel Start()
		{
			base.Start();

			_game = Models.Get<GameModel>();
			_tutorial = GameUI.Get<TutorialWindow>();

			return this;
		}

		public void StartTutorial(TutorialType tutorialType, int step = 0, bool blockedButtons = true)
		{
			if (!TutorialSteps.TryGetValue(tutorialType, out var stepsType)) return;
			
			Steps = (TutorialStepsBase)Activator.CreateInstance(stepsType);
			Steps.StartStep(step, (int) tutorialType, blockedButtons);

			AppEventsProvider.Action += OnGameAction;
			BaseButton.AnyButtonClickedEvent += OnButtonPressed;
			BaseWindow.WindowOpenedEvent += OnWindowOpened;
			BaseWindow.WindowClosedEvent += OnWindowClosed;

			IsActive = true;
		}
		
		public void FinishStep()
		{
			OnStepFinished();
		}

		private void OnGameAction(Enum type, object[] parameters)
		{
			if (type is GameEvents gameEvent && Steps.NextStepCondition == TutorialNextStepCondition.Action &&
			    Steps.NextStepConditionParam is GameEvents conditionEvent &&
			    gameEvent == conditionEvent)
			{
				OnStepFinished();
			}
		}

		private void OnTaskFinished(int id)
		{
			if (Steps == null) return;
			if (Steps.NextStepCondition == TutorialNextStepCondition.FinishedTask 
			    && id == (int) Steps.NextStepConditionParam)
			{
				OnStepFinished();
			}
		}

		private void OnWindowOpened(BaseUI window)
		{
			if (Steps.NextStepCondition == TutorialNextStepCondition.UIOpened &&
			    Steps.NextStepConditionParam is Type type &&
			    type == window.GetType())
			{
				OnStepFinished();
			}
		}

		private void OnWindowClosed(BaseUI window)
		{
			if (Steps.NextStepCondition == TutorialNextStepCondition.UIClosed &&
			    Steps.NextStepConditionParam is Type type &&
			    type == window.GetType())
			{
				OnStepFinished();
			}
		}

		private void OnButtonPressed()
		{
			if (Steps.NextStepCondition == TutorialNextStepCondition.PressedButton)
			{
				OnStepFinished();
			}
		}

		private void OnStepFinished()
		{
			if (Steps.LastStepIsJustStarted) return;

			_tapStartTime = 0f;

			if (_tutorial.IsPlayingTipAnim)
			{
				_tutorial.ForceTip();
			}
			else
			{
				if (Steps.IsLastStep)
				{
					OnFinished();
				}
				else
				{
					Steps.StartStep(Steps.CurrentStepIndex + 1);
				}
			}
		}

		private void OnHideDelayTips()
		{
			if (_tutorial.IsPlayingTipAnim)
			{
				_tutorial.ForceTip();
			}
			else
			{
				if (_tutorial.IsPlayingTipsDelay)
				{
					_tutorial.HideAll();
				}
			}
		}

		public override void Update(float deltaTime)
		{
			if (_tutorial.IsPlayingTipsDelay)
			{
				_tutorial.UpdateMe(deltaTime);
				CheckTap();
			}

			if (!IsActive) return;
			
			CheckTap();

			base.Update(deltaTime);
		}

		private void CheckTap()
		{
			if (Input.GetMouseButtonDown(0))
			{
				_tapStartTime = Time.realtimeSinceStartup;
			}
			else if (Input.GetMouseButtonUp(0) &&
			         Time.realtimeSinceStartup - _tapStartTime < TAP_TIME &&
			         Steps.NextStepCondition == TutorialNextStepCondition.Tap)
			{
				OnStepFinished();
			}
			// else if (Input.GetMouseButtonUp(0) &&
			//          Time.realtimeSinceStartup - _tapStartTime < TAP_TIME &&
			//          _tutorial.IsTipShowed && _tutorial.IsPlayingTipsDelay)
			// {
			// 	OnHideDelayTips();
			// }
		}

		private void OnFinished()
		{
			IsActive = false;
			Steps.FinishTutorial();
			TutorEnd?.Invoke();
			
			AppEventsProvider.Action -= OnGameAction;
			BaseButton.AnyButtonClickedEvent -= OnButtonPressed;
			BaseWindow.WindowOpenedEvent -= OnWindowOpened;
			BaseWindow.WindowClosedEvent -= OnWindowClosed;
		}
	}
}