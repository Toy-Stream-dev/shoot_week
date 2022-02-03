using GeneralTools.Tools;
using Plugins.GeneralTools.Scripts.UI;
using UnityEngine;
using UnityEngine.UI;

namespace _Game.Scripts.UI.Windows
{
	public class LoadingWindow : BaseWindow
	{
		[SerializeField] private Slider _slider;
		[SerializeField] private Image _loading;
		[SerializeField] private AnimationCurve[] _emulateCurves;

		private float _emulateLoading;
		private float _openedTime;
		
		private AnimationCurve _emulateCurve;
		
		public void Open(float progress)
		{
			if (IsNotInitialized) Init();
			Open();
			UpdateProgress(progress);
		}

		public override void UpdateMe(float deltaTime)
		{
			if(_emulateLoading <= 0) return;

			_openedTime += deltaTime;
			if (_openedTime > _emulateLoading)
			{
				Close();
				return;
			}
			
			UpdateProgress(_emulateCurve.Evaluate(_openedTime / _emulateLoading));

			base.UpdateMe(deltaTime);
		}
		
		public void UpdateProgress(float progress)
		{
			//_slider.value = progress;
			_loading.fillAmount = progress;
		}

		public void EmulateLoading(float time)
		{
			_emulateLoading = time;
			_openedTime = 0f;
			_emulateCurve = _emulateCurves.RandomValue();
			UpdateProgress(0f);
			Open();
		}
	}
}