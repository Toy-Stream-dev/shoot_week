using System;
using System.Collections.Generic;
using System.Linq;
using _Game.Scripts.Balance;
using _Game.Scripts.Enums;
using _Game.Scripts.Model.Numbers;
using _Game.Scripts.Saves;
using GeneralTools.Model;
using GeneralTools.Tools;
using UnityEngine;

namespace _Game.Scripts.Model
{
	[Flags]
	public enum GameFlag
	{
		None = 0,
		FirstSession = 1 << 0,
		ShotTutorial = 2 << 0,
		InventoryTutorial = 3 << 0,
	}

	public class GameModelBase : BaseModel, ISerializationCallbackReceiver
	{
		[SerializeField] private GameFlag _serializedFlags;
		[SerializeField] private List<GameParam> _params = new List<GameParam>();
		[SerializeField] private List<GameProgress> _progresses = new List<GameProgress>();
		[SerializeField] private List<GameData> _datas = new List<GameData>();

		private readonly Queue<Action> _actionsQueue = new Queue<Action>();

		protected GameBalance Balance { get; private set; }
		
		public FlagsContainer<GameFlag> Flags { get; } = new FlagsContainer<GameFlag>();
		public GameData CurrentData { get; set; }

		public override BaseModel Init()
		{
			Balance = GameBalance.Instance;
			
			return base.Init();
		}

		protected void InitGameData()
		{
			CurrentData = _datas.FirstOrDefault();
			if (CurrentData != null) return;
			CurrentData = new GameData(1);
			_datas.Add(CurrentData);
		}

		public int GetCurrentRegion()
		{
			if (CurrentData != null) return CurrentData.Region;
			var data = _datas.FirstOrDefault();
			return data?.Region ?? 0;
		}

		protected void SetGameData(int region = -1)
		{
			if (region == -1)
			{
				CurrentData = _datas.FirstOrDefault();
				return;
			}
			
			CurrentData = _datas.FirstOrDefault(d => d.Region == region);
			if (CurrentData != null) return;
			CurrentData = new GameData(region);
			_datas.Add(CurrentData);
		}

		protected GameParam CreateParam(GameParamType type, float baseValue = 0f, bool updateParamValue = true)
		{
			var param = new GameParam(type, baseValue);
			_params.Add(param);
			
			return param;
		}
		
		public GameParam GetParam(GameParamType type, bool createIfNotExists = true)
		{
			var param = _params.Find(p => p.Type == type);

			if (param == null && createIfNotExists)
			{
				param = new GameParam(type);
				_params.Add(param);
			}

			return param;
		}

		public bool HasParam(GameParamType type) => GetParam(type, false) != null;

		public GameProgress CreateProgress(GameParamType type, BigNumber target, bool looped = true)
		{
			var progress = new GameProgress(type, target, looped);
			_progresses.Add(progress);
			return progress;
		}

		public GameProgress GetProgress(GameParamType type) => _progresses.Find(p => p.Type == type);

		public IEnumerable<GameParamType> GetCurrentParams()
		{
			return _params.Select(p => p.Type);
		}

		public override void Update(float deltaTime)
		{
			while (_actionsQueue.Count > 0)
			{
				var action = _actionsQueue.Dequeue();
				action.Invoke();
			}
		}

		protected void PostponeAction(Action action)
		{
			_actionsQueue.Enqueue(action);
		}

		protected void CopyFrom(GameModelBase source)
		{
			_params.CopyFrom(source._params);
			_progresses.CopyFrom(source._progresses);

			_datas = new List<GameData>();
			foreach (var data in source._datas)
			{
				var newData = new GameData(data.Region);
				newData.CopyFrom(data);
				_datas.Add(newData);
			}
			
			CurrentData = _datas.FirstOrDefault();
			
			Flags.CopyFrom(source._serializedFlags);
		}

		public virtual void OnBeforeSerialize()
		{
			_serializedFlags = Flags.Value;
		}

		public virtual void OnAfterDeserialize()
		{
			Flags.CopyFrom(_serializedFlags);
		}
	}
}