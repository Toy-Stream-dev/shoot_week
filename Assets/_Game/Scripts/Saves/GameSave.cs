using System;
using System.Collections.Generic;
using _Game.Scripts.Model;
using _Game.Scripts.Model.Items;
using _Game.Scripts.Model.Numbers;
using GeneralTools.Model;
using UnityEngine;

namespace _Game.Scripts.Saves
{
	public static class GameSave
	{
		private const string KEY = "snapshot",
		                     KEY_BACKUP = "snapshot_backup";

		public static CitySnapshot LastLoaded { get; private set; }

		public static void Save()
		{
			var snapshot = new CitySnapshot()
			{
				Time = DateTime.Now,
				Game = Models.Get<GameModel>(),
				Items = Models.Get<ItemsModel>(),
			};

			Save(snapshot);
		}

		public static void Save(CitySnapshot snapshot)
		{
			var json = JsonUtility.ToJson(snapshot);
			PlayerPrefs.SetString(KEY, json);
			PlayerPrefs.Save();
		}

		public static bool Exists()
		{
			return PlayerPrefs.HasKey(KEY);
		}

		public static bool BackupExists()
		{
			return PlayerPrefs.HasKey(KEY_BACKUP);
		}

		public static CitySnapshot Load()
		{
			if (!Exists()) return null;

			var json = PlayerPrefs.GetString(KEY);
			LastLoaded = JsonUtility.FromJson<CitySnapshot>(json);

			return LastLoaded;
		}

		public static void DeleteSave()
		{
			PlayerPrefs.DeleteKey(KEY);
			PlayerPrefs.Save();
		}

		public static void Backup()
		{
			if (Exists())
			{
				PlayerPrefs.SetString(KEY_BACKUP, PlayerPrefs.GetString(KEY));
				PlayerPrefs.Save();
			}
		}

		public static void Restore()
		{
			if (BackupExists())
			{
				PlayerPrefs.SetString(KEY, PlayerPrefs.GetString(KEY_BACKUP));
				PlayerPrefs.Save();
			}
		}

		public static void CopyFrom(this List<GameParam> destination, List<GameParam> source)
		{
			foreach (var sourceParam in source)
			{
				var destinationParam = destination.Find(p => p.Type == sourceParam.Type);
				if (destinationParam == null)
				{
					destinationParam = new GameParam(sourceParam.Type);
					destination.Add(destinationParam);
				}

				destinationParam.CopyFrom(sourceParam);
			}
		}

		public static void CopyFrom(this List<GameProgress> destination, List<GameProgress> source)
		{
			foreach (var sourceProgress in source)
			{
				var destinationParam = destination.Find(p => p.Type == sourceProgress.Type);
				if (destinationParam == null)
				{
					destinationParam = new GameProgress(sourceProgress.Type, sourceProgress.TargetValue, sourceProgress.Looped);
					destination.Add(destinationParam);
				}

				destinationParam.CopyFrom(sourceProgress);
			}
		}
	}
}