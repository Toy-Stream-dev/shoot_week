using UnityEngine;
using UnityEngine.UI;

namespace _Game.Scripts.UI.Progress
{
	public class CircleProgressUI : ProgressItemUI
	{
		[SerializeField] private Image _fill;

		protected override void Redraw()
		{
			_fill.fillAmount = Progress == null ? 0f : (float)Progress.ProgressValue;
		}
		
		public override void Destroy()
		{
			if (Progress == null) return;
			base.Destroy();
		}
	}
}