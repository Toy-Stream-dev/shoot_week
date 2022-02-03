using System;
using System.Linq;
using _Game.Scripts.Balance;
using _Game.Scripts.Enums;
using _Game.Scripts.Model.Numbers;
// using _Idle.Scripts.Model.Boosts;
// using _Idle.Scripts.Model.Numbers;
using GeneralTools.Model;
using GeneralTools.UI;
using UnityEngine;

namespace _Game.Scripts.Model
{
	public class MathModel : BaseModel
	{
		private static GameBalance _balance;
		//private static BoostsModel _boosts;
		private GameParam _fish;
		
		public override BaseModel Init()
		{
			_balance = GameBalance.Instance;
			//_boosts = Models.Get<BoostsModel>();

			return base.Init();
		}
		

		// public double GetValue(TaskType type, GameParam param, int region)
		// {
		// 	var progression = _balance.Progressions.Find(p => p.Type == param.Type);
		// 	if (progression == null) return 1;
		// 	
		// 	if (param.Level == 1) return progression.Round ? Math.Round(progression.Base) : progression.Base;
		//
		// 	double value = 0;
		//
		// 	return progression.Round ? Math.Round(value) : value;
		// }

		public double GetValue(GameParam param, bool forNextLevel = false)
		{
			var progression = _balance.Progressions.Find(p => p.Type == param.Type);
			var level = param.Level;
			if (forNextLevel) level++;
			double value = 0;
			value = progression.GetValue(param.Level);

			return progression.Round ? Math.Round(value) : value;
		}

		public double GetValue(GameParamType param, int level)
		{
			var progression = _balance.Progressions.Find(p => p.Type == param);

			var progressionValue = progression.GetValue(level, true);
			return progression.Round ? Math.Round(progressionValue) : progressionValue;
		}

		public static (double, int) GetMantissaAndExp(double value)
		{
			var exp = 0;
			double expValue = 1000;

			if (value < expValue) return (value, 0);

			while (value >= expValue)
			{
				exp++;
				expValue *= 1000;
			}

			expValue /= 1000;

			return (value / expValue, exp);
		}

		public static double GetRoundedValue(double value)
		{
			var (v, exp) = GetMantissaAndExp(value);
			return ((int)v) * Mathf.Pow(1000, exp);
		}
	}
}