using GeneralTools.Localization;
using GeneralTools.UI;
using TMPro;
using UnityEngine;

namespace _Game.Scripts.UI.Progress
{
	public class TextProgressUI : ProgressItemUI
	{
		[SerializeField] private TextMeshProUGUI _text;
		[SerializeField] private TextAutoSize _textAutoSize;

		public override void SetProgress(_Game.Scripts.Model.Numbers.Progress progress)
		{
			base.SetProgress(progress);
			Localization.LanguageChangedEvent += Redraw;
		}

		public override void Destroy()
		{
			Localization.LanguageChangedEvent -= Redraw;
			if (Progress == null) return;
			base.Destroy();
		}

		protected override void Redraw()
		{
			if (Progress == null)
			{
				SetText("0 / 0");
				return;
			}

			SetText($"{Progress.CurrentValue.ToString()} / {Progress.TargetValue.ToString()}");
		}

		private void SetText(string text)
		{
			if (_textAutoSize != null)
			{
				_textAutoSize.Text = text;
			}
			else
			{
				_text.text = text;
			}
		}
	}
}