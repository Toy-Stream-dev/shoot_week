using System;
using _Game.Scripts.Balance;
using _Game.Scripts.Enums;
using UnityEngine;

namespace _Game.Scripts.Model.Numbers
{
	[Serializable]
	public class GameParam : BigNumber
	{
		public event Action LevelChanged;

		[SerializeField] private GameParamType _type;
		[SerializeField] private int _level = 1;

		public GameParamType Type => _type;
		public int Level => _level;

		public GameParam(GameParamType type)
		{
			_type = type;
		}

		public GameParam(GameParamType type, float value)
		{
			_type = type;
			Init(value);
		}

		public bool IsTrue()
		{
			return Value > 0;
		}

		public bool IsFalse()
		{
			return !IsTrue();
		}

		public void IncLevel()
		{
			_level++;
			LevelChanged?.Invoke();
		}

		public void SetLevel(int level)
		{
			_level = level;
			LevelChanged?.Invoke();
		}

		public void CopyFrom(GameParam source)
		{
			base.CopyFrom(source);

			if (_level != source._level)
			{
				_level = source._level;
				LevelChanged?.Invoke();
			}
		}
		
		public override string ToString()
		{
			return $"({_type}: {Value})";
		}
	}
}