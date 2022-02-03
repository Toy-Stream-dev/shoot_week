using _Game.Scripts.Balance;
using _Game.Scripts.Saves;
using _Game.Scripts.Tools;
using _Game.Scripts.View;
using UnityEditor;
using UnityEngine;
using static GeneralTools.Editor.EditorUITools;

namespace _Game.Scripts.Editor
{
	public class DevToolsWindow : EditorWindow
	{
		private string _command;

		[MenuItem("_Game/Dev tools", false, -10)]
		
		static void Init()
		{
			var window = (DevToolsWindow)GetWindow(typeof(DevToolsWindow));
			window.Show();
			window.titleContent.text = "Dev tools";
		}

		private void OnGUI()
		{
			if (Button("Run test")) TestTools.RunMagicTest();
			
			BeginHorizontal();
			if (Button("Balance")) Select(GameBalance.Instance);
			if (Button("Battle")) Select(BattleSettings.Instance);
			EndHorizontal();
			Space();
			DrawSaves();
		}

		private void DrawSaves()
		{
			BeginHorizontal();

			if (Application.isPlaying)
			{
				if (Button("Save")) GameSave.Save();
			}
			else
			{
				if (GameSave.Exists() && Button("Delete")) GameSave.DeleteSave();
				if (GameSave.BackupExists() && Button("Restore")) GameSave.Restore();
			}

			if (GameSave.Exists() && Button("Backup")) GameSave.Backup();

			EndHorizontal();
			Space();
		}

		private void Select(Object obj)
		{
			Selection.activeObject = obj;
		}
	}
}