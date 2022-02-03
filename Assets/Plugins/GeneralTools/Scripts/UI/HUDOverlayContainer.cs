using GeneralTools.Tools;

namespace Plugins.GeneralTools.Scripts.UI
{
	public class HUDOverlayContainer : BaseWindow
	{
		public override void Init()
		{
			base.Init();
			gameObject.Activate();
		}
	}
}