using System.Collections.Generic;
using GeneralTools.Localization;
using GoogleParse;
using UnityEngine;

namespace _Game.Scripts.Tools
{
	[CreateAssetMenu(fileName = "LocalizationsContainer", menuName = "_Game/LocalizationsContainer", order = 0)]
	public class LocalizationsContainer : global::GeneralTools.Localization.LocalizationsContainer, IParsable
	{
		[HideInInspector] public List<LocalizationData> Exp, Tutorial;

		public override void OnParsed()
		{
			Localization.AddRange(Exp);
			Localization.AddRange(Tutorial);
			base.OnParsed();
		}
	}
}