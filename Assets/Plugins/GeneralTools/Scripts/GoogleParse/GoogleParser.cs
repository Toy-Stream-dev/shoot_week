//Сделано по этому туториалу https://developers.google.com/sheets/api/quickstart/js
//Создать приложение можно здесь https://console.developers.google.com/projectcreate
//Информацию о приложениях можно посмотреть тут https://console.developers.google.com/home/dashboard

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GoogleParse.BalanceParse;
using Sirenix.OdinInspector;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GoogleParse
{
	[CreateAssetMenu(fileName = "GoogleParser", menuName = "GoogleParser", order = 1)]
	public class GoogleParser : ScriptableObject
	{
		private static GoogleParser _instance;

		[Serializable]
		public class TableInfo
		{
			public string Name;
			public ScriptableObject ScriptableObject;
			public string SpreadSheetLink;
		}

		[SerializeField] private TextAsset _credentials;
		[SerializeField, HideInInspector] private string _applicationName;
		[SerializeField] private List<TableInfo> _tableInfos;

		public static GoogleParser Instance
		{
			get
			{
				if (_instance == null)
				{
					_instance = (GoogleParser)Resources.LoadAll("", typeof(GoogleParser)).FirstOrDefault();
				}

				return _instance;
			}
		}

#if ODIN_INSPECTOR
		[Button("Parse", ButtonSizes.Large)]
#endif
		public void ParseAllTables()
		{
			foreach (var tableInfo in _tableInfos)
			{
				ParseTable(tableInfo);
			}
		}

		public IEnumerable<string> GetTableNames()
		{
			return _tableInfos.Select(t => t.Name);
		}

		public void ParseTable(string tableName)
		{
			var tableInfo = _tableInfos.Find(i => i.Name == tableName);
			ParseTable(tableInfo);
		}

		private void ParseTable(TableInfo tableInfo)
		{
			if (tableInfo == null)
			{
				Debug.LogError($"Can't parse null table info");
				return;
			}

			var tableName = tableInfo.Name;
			var folderName = $"~{tableName}";
			var folderPath = $"{Application.dataPath}/{folderName}";

			DownloadSpreadSheet(tableInfo.SpreadSheetLink, folderPath);

			if (!Directory.Exists(folderPath))
			{
				Debug.LogError($"Directory \"{folderPath}\" not found");
				return;
			}

			ParseCSVToObject(tableInfo.ScriptableObject, folderPath, tableName);

			Directory.Delete(folderPath, true);

			Debug.Log($"{tableName} parsed");
			
#if UNITY_EDITOR
			UnityEditor.Selection.activeObject = tableInfo.ScriptableObject;
#endif
		}

		private void DownloadSpreadSheet(string spreadSheet, string folderPath)
		{
#if UNITY_EDITOR
			//GoogleCSVDownloader.Init(_applicationName, UnityEditor.AssetDatabase.GetAssetPath(_credentials));
			//GoogleCSVDownloader.DownloadSheetsAsCSV(spreadSheet, folderPath);
#endif
		}

		private void ParseCSVToObject(Object toObj, string folderPath, string workbookName)
		{
			var files = Directory.GetFiles(folderPath, "*.csv");
			var workbook = CSVReader.ParseCSV(workbookName, files, '\t');

			ValuesImporter.Import(toObj, workbook);
			
#if UNITY_EDITOR
			UnityEditor.EditorUtility.SetDirty(toObj);
			UnityEditor.Selection.activeObject = toObj;
#endif
		}
	}
}