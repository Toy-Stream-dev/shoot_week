using System;
using DG.Tweening;
using GeneralTools.Localization;
using GeneralTools.Model;
using GeneralTools.Tools;
using GeneralTools.UI;
using Plugins.GeneralTools.Scripts.UI;
using TMPro;
using UnityEngine;

namespace _Game.Scripts.Tutorial.UI
{
	public class TutorialWindow : BaseWindow
	{
		//[SerializeField] private GameObject _tutorPanel;
		//[SerializeField] private GameObject _tipsContainer;
		[SerializeField] private RectTransform _swipeTutor;
		//[SerializeField] private TextMeshProUGUI _tipText;
		//[SerializeField] private TextMeshProUGUI _tapText;
		//[SerializeField] private TutorialArrow _arrow;
		[SerializeField] private GameObject _touchBlocker;

		[SerializeField] private BaseButton _continue;

		private Tween _textTween;

		private const float _tipCharAnimDuration = 0.03f;
		private float _delay;
		
		private int _tipChangingFrame;

		public bool IsPlayingTipAnim => _textTween?.IsPlaying() ?? false;
		public bool IsPlayingTipsDelay { get; private set; }
		//public bool IsTipShowed => _tipsContainer.activeSelf;
		//public bool IsPanelShowed => _tutorPanel.activeSelf;
		//public GameObject TutorialPanel => _tutorPanel;

		public BaseButton Continue => _continue;

		//private SceneInteractionModel _sceneInteraction;

		public override void Init()
		{
			//_tipsContainer.Deactivate();
			base.Init();
		}

		public override BaseUI Open()
		{
			//_sceneInteraction = Models.Get<SceneInteractionModel>();
			//_tapText.text = "tap".Localized();
			HideAll();
			AppEventsProvider.Action += AppEventsProviderOnAction;
			return base.Open();
		}

		private void AppEventsProviderOnAction(Enum type, object[] parameters)
		{
			
		}

		// public void ShowTipWithDelay(string text, float delay, Vector2 fingerPos)
		// {
		// 	_delay = delay;
		// 	_tipText.text = text.Localized();
		// 	_finger.rectTransform.anchoredPosition = fingerPos;
		// 	
		// 	_tipsContainer.Deactivate();
		// 	_finger.Deactivate();
		//
		// 	IsPlayingTipsDelay = true;
		// }

		public override void UpdateMe(float deltaTime)
		{
			if (_delay <= 0) return;

			_delay -= deltaTime;
			if (_delay >= 0) return;
			
			//_tipsContainer.Activate();
			//_finger.Activate();

			//PlayTipAnim();
			
			this.Activate();
			
			base.UpdateMe(deltaTime);
		}

		// public void ShowTip(string text, bool localize = false)
		// {
		// 	_tipsContainer.Activate();
		// 	_tipText.text = localize ? text.Localized() : text;
		// 	PlayTipAnim();
		// }

		public void ShowShotTutor()
		{
			_swipeTutor.Activate();
		}

		// private void PlayTipAnim()
		// {
		// 	KillAnim();
		//
		// 	_tipText.maxVisibleCharacters = 0;
		// 	_tipChangingFrame = Time.frameCount;
		//
		// 	//if (_tutorialTarget != null) _tutorialTarget.TemporaryDisableRaycast();
		//
		// 	_textTween = PlayTextAnim(_tipText,
		// 		_tipCharAnimDuration,
		// 		RestoreTargetFrameRaycast);
		// }
		
		private void RestoreTargetFrameRaycast()
		{
			_textTween = null;
			//if (_tutorialTarget != null) _tutorialTarget.RestoreRaycastState();
		}

		private static Tween PlayTextAnim(TMP_Text text,
			float charDuration = 0.03f,
			Action onComplete = null)
		{
			text.ForceMeshUpdate();
			var characterCount =  text.textInfo.characterCount;
			var duration = characterCount * charDuration;
			text.maxVisibleCharacters = 0;

			return DOTween.To(() => text.maxVisibleCharacters,
					value => text.maxVisibleCharacters = value,
					characterCount,
					duration)
				.SetUpdate(true)
				.SetEase(Ease.Linear)
				.OnComplete(() =>
				{
					onComplete?.Invoke();
					text.maxVisibleCharacters = int.MaxValue;
				});
		}
		
		private void KillAnim()
		{
			_textTween?.Kill();
			_textTween = null;
		}

		public void ForceTip()
		{
			if (_tipChangingFrame == Time.frameCount) return;

			_textTween?.Complete(false);
			_textTween = null;
			
			RestoreTargetFrameRaycast();
		}
		
		// private void HideTips()
		// {
		// 	_tipsContainer.Deactivate();
		// }

		public void ShowFingerAtWorldPos(Vector3 worldPos)
		{
			var screenPos = GameCamera.UnityCam.WorldToScreenPoint(worldPos);
			//ShowFingerAtScreenPos(screenPos);
		}

		// private void ShowFingerAtScreenPos(Vector2 screenPos)
		// {
		// 	_finger.Activate();
		// 	_finger.rectTransform.anchoredPosition = screenPos;
		// }
		//
		// private void HideFinger()
		// {
		// 	_finger.Deactivate();
		// }

		private void HideArrow()
		{
			//_arrow.Deactivate();
		}

		public void SetTouchBlockerState(bool active)
		{
			_touchBlocker.SetActive(active);

			//if (!blockTaps) return;

			// if (active)
			// {
			// 	_sceneInteraction.Flags.Set(InteractionFlags.IgnoreSceneTaps);
			// }
			// else
			// {
			// 	_sceneInteraction.Flags.Clear(InteractionFlags.IgnoreSceneTaps);
			// }
		}

		public void HideAll()
		{
			//HideTips();
			//HideFinger();
			_swipeTutor.Deactivate();
			HideArrow();
			SetTouchBlockerState(false);
			IsPlayingTipsDelay = false;
		}
	}
}