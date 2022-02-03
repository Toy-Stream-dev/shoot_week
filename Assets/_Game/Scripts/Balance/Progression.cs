using System;
using _Game.Scripts.Enums;
using _Game.Scripts.Model;
using GeneralTools.Tools.ExtensionMethods;
using UnityEngine;

namespace _Game.Scripts.Balance
{
	[Serializable]
	public class Progression
	{
		public GameParamType Type;
		public float Base, K1, K2;
		public int LevelCap;
		public bool Round;
		public int Digits;
		
		public double GetValue(int level, bool dontRound = false)
		{
			if (LevelCap > 0 && level > LevelCap)
			{
				level = LevelCap;
			}

			double result;

			if (K1.EqualTo(1f))
			{
				result = Base + K2 * (level - 1);
			}
			else
			{
				var k = Mathf.Pow(K1, level - 1);
				result = Base * k + K2 * (k - 1) * (1 / (K1 - 1));
			}

			if (Round && Digits > 0)
			{
				return Math.Round(result, Digits, MidpointRounding.AwayFromZero);
			}
			
			return !Round || dontRound ? result : Math.Round(result);
		}
		
		public double GetValue(int level, double value, bool dontRound = false)
		{
			if (LevelCap > 0 && level > LevelCap)
			{
				level = LevelCap;
			}

			double result;

			if (K1.EqualTo(1f))
			{
				result = value + K2 * (level - 1);
			}
			else
			{
				var k = Mathf.Pow(K1, level - 1);
				result = value * k + K2 * (k - 1) * (1 / (K1 - 1));
			}

			if (Round && Digits > 0)
			{
				return Math.Round(result, Digits, MidpointRounding.AwayFromZero);
			}
			
			return !Round || dontRound ? result : Math.Round(result);
		}

		public double GetRoundedValue(int level)
		{
			var value = GetValue(level);
			return MathModel.GetRoundedValue(value);
		}
	}
}