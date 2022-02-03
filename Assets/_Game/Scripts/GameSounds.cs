using System;
using System.Collections.Generic;
using _Game.Scripts.Enums;
using _Game.Scripts.Model.Numbers;
using GeneralTools;
using GeneralTools.UI;
using Plugins.GeneralTools.Scripts.UI;
using UnityEngine;

namespace _Game.Scripts
{
	[Serializable]
	public enum GameSoundType
	{
		None,
		ButtonClick,
		Open,
		Close,
		ShootPistol,
		ShootAutomaticRifle,
		ShootShotgun,
		MeleeWeapon,
		Grenade,
		MainTheme,
		ShootUZI,
		ShootMagnum,
		MeleeWeaponSlash,
		Hud,
		Soft,
		Katana,
		Machete,
		Clever,
		Bat,
		Baton,
		Crowbar,
		MedPack,
		PickUP,
		Runduk,
	}

	[Serializable]
	public class GameSound
	{
		public GameSoundType Type;
		public AudioClip Clip;
		public float RepeatDelay;
		public bool RandomizePitch;
		[NonSerialized] public GameProgress Progress;
	}

	public class GameSounds : BaseBehaviour
	{
		public static GameSounds Instance { get; private set; }

		[SerializeField] private List<GameSound> _sounds;
		[SerializeField] private AudioSource _sourceMusic;
		[SerializeField] private AudioSource _sourceSound;

		private void Awake()
		{
			Instance = this;
		}

		public void Init()
		{
			BaseButton.ClickSoundEvent += _ => PlaySound(GameSoundType.ButtonClick);
			BaseWindow.WindowOpenedEvent += _ => PlaySound(GameSoundType.Open);
			BaseWindow.WindowClosedEvent += _ => PlaySound(GameSoundType.Close);
			GameSettings.Instance.SoundChangedEvent += UpdateSoundsSettings;

			foreach (var sound in _sounds)
			{
				sound.Progress = new GameProgress(GameParamType.Timer, sound.RepeatDelay,false);
				//if (sound.RepeatDelay <= 0) sound.Progress.Pause();
			}

			UpdateSoundsSettings();
		}

		public override void UpdateMe(float deltaTime)
		{
			foreach (var sound in _sounds)
			{
				sound.Progress.Change(deltaTime);
			}
			
			base.UpdateMe(deltaTime);
		}

		private void UpdateSoundsSettings()
		{
			_sourceMusic.mute = GameSettings.Instance.IsMuteMusic;
			_sourceSound.mute = GameSettings.Instance.IsMuteSound;
		}

		public void PlayMusic(GameSoundType gameSoundType)
		{
			if (GameSettings.Instance.IsMuteMusic) return;
			
			var clip = _sounds.Find(s => s.Type == gameSoundType)?.Clip;
			if (clip == null) return;
			_sourceMusic.clip = clip;
			_sourceMusic.loop = true;
			_sourceMusic.time = 0f;
			_sourceMusic.Play();
		}
		
		public void PlaySound(GameSoundType gameSoundType)
		{
			if (GameSettings.Instance.IsMuteSound) return;
			var sound = _sounds.Find(s => s.Type == gameSoundType);
			if(sound == null) return;
			if (sound.Progress.TargetValue > 0)
			{
				if (!sound.Progress.IsCompleted) return;
				sound.Progress.Reset();	
			}
			var clip = sound.Clip;
			// if (sound.RandomizePitch)
			// {
			// 	_sourceSound.pitch = 1 + Random.Range(-0.1f, 0.1f);
			// }
			// else
			// {
			// 	_sourceSound.pitch = 1;
			// }
			if (clip != null)
			{
				_sourceSound.PlayOneShot(clip);
			}
		}
	}
}