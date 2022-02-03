using GeneralTools.Editor;
using UnityEditor;

namespace _Game.Scripts.Editor
{
	public class DevFlagsWindow : DevFlagsWindow<DevFlags>
	{
		[MenuItem("_Game/Dev flags", false, -10)]
		public static void Init()
		{
			((DevFlagsWindow)GetWindow(typeof(DevFlagsWindow))).Show();
		}
	}
}