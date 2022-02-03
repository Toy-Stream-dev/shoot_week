using System;
using System.Globalization;
using _Game.Scripts.Model;
using _Game.Scripts.Model.Items;
using UnityEngine;

namespace _Game.Scripts.Saves
{
	public class CitySnapshot : ISerializationCallbackReceiver
	{
		public GameModel Game;
		public DateTime Time;
		public ItemsModel Items;
		public int Version;

		[SerializeField] private string _gameStr = string.Empty;
		[SerializeField] private string _timeStr = string.Empty;
		[SerializeField] private string _itemsStr = string.Empty;
		[SerializeField] private string _versionStr = string.Empty;

		public void OnBeforeSerialize()
		{
			_gameStr = JsonUtility.ToJson(Game);
			_timeStr = Time.ToString(CultureInfo.InvariantCulture);
			_itemsStr = JsonUtility.ToJson(Items);
			_versionStr = "1";
		}

		public void OnAfterDeserialize()
		{
			Game = JsonUtility.FromJson<GameModel>(_gameStr);
			Time = DateTime.Parse(_timeStr, CultureInfo.InvariantCulture);
			Items = JsonUtility.FromJson<ItemsModel>(_itemsStr);
			
			int.TryParse(_versionStr, out var version);
			Version = version;
		}
	}
}