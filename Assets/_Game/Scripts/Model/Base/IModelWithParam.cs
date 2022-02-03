using System.Collections.Generic;
using _Game.Scripts.Enums;
using _Game.Scripts.Model.Numbers;

namespace _Game.Scripts.Model.Base
{
	public interface IModelWithParam
	{
		GameParam GetParam(GameParamType type, bool createIfNotExists = true);
		IEnumerable<GameParamType> GetCurrentParams();
	}
}