using _Game.Scripts.Model.Numbers;
using UnityEngine;

namespace _Game.Scripts.UI.Progress
{
	public class ProgressItemUI : GameParamUI
	{
		protected _Game.Scripts.Model.Numbers.Progress Progress { get; private set; }

		public virtual void SetProgress(_Game.Scripts.Model.Numbers.Progress progress)
		{
			if (progress == null)
			{
				Debug.Log($"Cannot set null progress");
				return;
			}

			if (progress is GameProgress gameProgress)
			{
				ParamType = gameProgress.Type;
				if (gameProgress.Target is GameParam targetParam)
				{
					targetParam.LevelChanged += Redraw;
				}
			}

			Progress = progress;
			Progress.UpdatedEvent += Redraw;

			Redraw();
		}

		public virtual void Destroy()
		{
			if (Progress == null) return;
			if (Progress.Target is GameParam targetParam)
			{
				targetParam.LevelChanged -= Redraw;
			}
			
			Progress.UpdatedEvent -= Redraw;
		}

		protected virtual void Redraw()
		{
		}
	}
}