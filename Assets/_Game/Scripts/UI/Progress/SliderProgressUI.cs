using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Game.Scripts.UI.Progress
{
	public class SliderProgressUI : ProgressItemUI
	{
		[SerializeField] private Slider _slider;
		[SerializeField] private TextMeshProUGUI _text;

		protected override void Redraw()
		{
			_slider.value = Progress == null ? 0f : (float)Progress.ProgressValue;
			if (_text != null) _text.text = Progress == null ? "" : $"{Progress.ProgressValue * 100}%";
		}
	}
}