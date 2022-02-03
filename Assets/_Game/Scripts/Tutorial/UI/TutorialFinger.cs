using _Game.Scripts.Balance;
using _Game.Scripts.Enums;
using _Game.Scripts.UI.HUD;
using DG.Tweening;
using GameAnalyticsSDK.Setup;
using GeneralTools;
using GeneralTools.Tools;
using GeneralTools.UI;
using UnityEngine;

namespace _Game.Scripts.Tutorial.UI
{
	public enum FingerAnimation
	{
		None,
		Swipe,
		Click,
	}
	
	public class TutorialFinger : BaseUIBehaviour
	{
		[SerializeField] private Vector2 _buttonAnchoredPosition = new Vector2(0, 30);
		[SerializeField] private Animator _animator;

		public static TutorialFinger CurrentFinger { get; set; }
		private BaseButton TargetButton { get; set; }

		public static void ShowOnHud(GamePlayElement element)
		{
			var target = GameUI.Get<HUD>().GetGameplayElement(element);
			if (target == null) return;
			Show(target);
		}
		
		public static void ShowOnHud(BaseButton element, Vector3 startPosition = default, Vector2 position = default)
		{
			Show(element, startPosition, position);
		}

		public static void Show(BaseUIBehaviour behaviour, Vector2 position = default)
		{
			var button = behaviour.GetComponentInChildren<BaseButton>(true);
			if (button == null) return;

			if (CurrentFinger != null) CurrentFinger.Remove();

			CurrentFinger = Prefabs.CopyPrefab<TutorialFinger>();
			CurrentFinger.Init(button, position);
		}

		public static void PlayAnimation(FingerAnimation fingerAnimation)
		{
			switch (fingerAnimation)
			{
				case FingerAnimation.Swipe:
					CurrentFinger._animator.Play("Swipe");
					break;
				case FingerAnimation.Click:
					CurrentFinger._animator.Play("Click");
					break;
			}
		}
		
		public static void ShowOnUi(BaseUIBehaviour behaviour, Vector2 position = default)
		{
			if (CurrentFinger != null) CurrentFinger.Remove();

			CurrentFinger = Prefabs.CopyPrefab<TutorialFinger>();
			CurrentFinger.Init(behaviour, position);
		}

		private static void Show(BaseButton button, Vector3 startPosition, Vector2 position = default)
		{
			//if (CurrentFinger != null) CurrentFinger.Remove();

			if (CurrentFinger == null)
			{
				CurrentFinger = Prefabs.CopyPrefab<TutorialFinger>();	
			}
			CurrentFinger.Init(button, startPosition, position);
		}
		
		public static void SetParent(Transform parent)
		{
			CurrentFinger.SetParent(parent);
		}
		
		private void Init(BaseButton button, Vector3 startPosition = default, Vector2 position = default)
		{
			if (startPosition == default)
			{
				rectTransform.SetParent(button.rectTransform, false);
			}
			//rectTransform.SetParent(button.rectTransform, false);

			var anchoredPosition = _buttonAnchoredPosition;
			var scale = button.rectTransform.localScale;
			var rotate = button.rectTransform.rotation;
			
			if (button.TryGetComponent(out CustomTutorialFingerAnchor customTutorialFingerAnchor))
			{
				anchoredPosition = customTutorialFingerAnchor.Anchor;
				scale = customTutorialFingerAnchor.Scale;
				rotate = customTutorialFingerAnchor.Rotation;
			}
			
			if (position != default)
			{
				anchoredPosition = position;
			}

			// if (startPosition == default)
			// {
			// 	rectTransform.SetParent(button.rectTransform, true);
			// 	
			// 	rectTransform.anchoredPosition = anchoredPosition;
			// 	rectTransform.localScale = scale;
			// 	rectTransform.rotation = rotate;
			// }
			// else
			// {
				rectTransform.SetParent(GameUI.Root, false);
				if (startPosition == default)
				{
					rectTransform.anchoredPosition = Vector2.zero;
				}
				else
				{
					rectTransform.transform.position = startPosition;
				}
				DOTween.Sequence().Append(rectTransform.DOMove(button.transform.position, GameBalance.Instance.FingerDelay)).OnComplete(() =>
				{
					PlayAnimation(FingerAnimation.Click);
					rectTransform.SetParent(button.rectTransform, true);
					rectTransform.anchoredPosition = anchoredPosition;
				});
				rectTransform.localScale = scale;
				rectTransform.rotation = rotate;
			//}

			TargetButton = button;
			//button.ClickedEvent += Remove;
		}
		
		private void Init(BaseUIBehaviour parent, Vector2 position = default)
		{
			rectTransform.SetParent(parent.rectTransform, false);
			
			var anchoredPosition = _buttonAnchoredPosition;
			var scale = parent.rectTransform.localScale;
			var rotate = parent.rectTransform.rotation;
			
			if (parent.TryGetComponent(out CustomTutorialFingerAnchor customTutorialFingerAnchor))
			{
				anchoredPosition = customTutorialFingerAnchor.Anchor;
				scale = customTutorialFingerAnchor.Scale;
				rotate = customTutorialFingerAnchor.Rotation;
			}
			
			if (position != default)
			{
				anchoredPosition = position;
			}
			
			rectTransform.anchoredPosition = anchoredPosition;
			rectTransform.localScale = scale;
			rectTransform.rotation = rotate;
		}

		private void SetFingerParent(Transform parent)
		{
			CurrentFinger.SetParent(parent);
		}
		
		public static void RemoveFinger()
		{
			if (CurrentFinger != null) CurrentFinger.Remove();
		}
		
		private void Remove()
		{
			if (TargetButton != null)
			{
				TargetButton.ClickedEvent -= Remove;
			}

			this.DestroyGO();

			if (CurrentFinger == this) CurrentFinger = null;
		}
	}
}