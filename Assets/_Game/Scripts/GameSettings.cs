using System;
using UnityEngine;

namespace _Game.Scripts
{
	public class GameSettings
	{
		private const string KEY = "settings";

		public event Action SoundChangedEvent;
		public bool IsMuteMusic => _muteMusic;
		public bool IsMuteSound => _muteSound;
		public bool IsVibrationOff => _vibration;
		
		private static GameSettings _instance;
		
		[SerializeField] private bool _muteMusic;
		[SerializeField] private bool _muteSound;
		[SerializeField] private bool _vibration;

		public bool MuteMusic
		{
			get => _muteMusic;
			set => SetMuteMusic(value);
		}
		
		public bool MuteSound
		{
			get => _muteSound;
			set => SetMuteSound(value);
		}
		
		public bool Vibration
		{
			get => _vibration;
			set => SetVibration(value);
		}
		
		public static GameSettings Instance
		{
			get
			{
				if (_instance == null) Load();
				return _instance;
			}
		}
		
		private void SetMuteMusic(bool mute)
		{
			_muteMusic = mute;
			Save();
			SoundChangedEvent?.Invoke();
		}
		
		private void SetMuteSound(bool mute)
		{
			_muteSound = mute;
			Save();
			SoundChangedEvent?.Invoke();
		}
		
		private void SetVibration(bool flag)
		{
			_vibration = flag;
			Save();
			SoundChangedEvent?.Invoke();
		}
		
		public static void Load()
		{
			if (PlayerPrefs.HasKey(KEY))
			{
				var json = PlayerPrefs.GetString(KEY);
				_instance = JsonUtility.FromJson<GameSettings>(json);
			}
			else
			{
				_instance = new GameSettings();
			}
		}

		private static void Save()
		{
			if (_instance == null) Load();
			var json = JsonUtility.ToJson(_instance);
			PlayerPrefs.SetString(KEY, json);
			PlayerPrefs.Save();
		}
	}
}