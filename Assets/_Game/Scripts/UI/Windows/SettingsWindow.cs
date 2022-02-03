using GeneralTools.Localization;
using GeneralTools.UI;
using Plugins.GeneralTools.Scripts.UI;
using TMPro;
using UnityEngine;

namespace _Game.Scripts.UI.Windows
{
	public class SettingsWindow : BaseWindow
	{
		[SerializeField] private TextMeshProUGUI _title;
		[SerializeField] private TextMeshProUGUI _musicText;
		[SerializeField] private TextMeshProUGUI _soundText;
		[SerializeField] private TextMeshProUGUI _vibrationText;
		[SerializeField] private TextMeshProUGUI _languageText;
		
		[SerializeField] private BaseButton _musicButton;
		[SerializeField] private BaseButton _soundButton;
		[SerializeField] private BaseButton _vibrationButton;

		public override void Init()
		{
			_musicButton.SetCallback(OnPressedMusicButton);
			_soundButton.SetCallback(OnPressedSoundButton);
			_vibrationButton.SetCallback(OnPressedVibrationButton);

			base.Init();
		}

		public override BaseUI Open()
		{
			var settings = GameSettings.Instance;
			_musicButton.Sprite = settings.IsMuteMusic ? "Off".GetSprite() : "On".GetSprite();
			_soundButton.Sprite = settings.IsMuteSound ? "Off".GetSprite() : "On".GetSprite();
			_vibrationButton.Sprite = settings.IsVibrationOff ? "Off".GetSprite() : "On".GetSprite();

			return base.Open();
		}

		public override void UpdateLocalization()
		{
			_title.text = "setting".Localized();
			_musicText.text = "music".Localized();
			_soundText.text = "sound".Localized();
			_vibrationText.text = "vibration".Localized();
			_languageText.text = "language".Localized();
			
			base.UpdateLocalization();
		}

		private void OnPressedMusicButton()
		{
			var settings = GameSettings.Instance;
			settings.MuteMusic = !settings.IsMuteMusic;
			_musicButton.SetSprite(settings.IsMuteMusic ? "Off".GetSprite() : "On".GetSprite());
		}
		
		private void OnPressedSoundButton()
		{
			var settings = GameSettings.Instance;
			settings.MuteSound = !settings.IsMuteSound;
			_soundButton.SetSprite(settings.IsMuteSound ? "Off".GetSprite() : "On".GetSprite());
		}
		
		private void OnPressedVibrationButton()
		{
			var settings = GameSettings.Instance;
			settings.Vibration = !settings.IsVibrationOff;
			_vibrationButton.SetSprite(settings.IsVibrationOff ? "Off".GetSprite() : "On".GetSprite());
		}
	}
}