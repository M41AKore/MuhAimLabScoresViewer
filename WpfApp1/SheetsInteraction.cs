using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MuhAimLabScoresViewer
{
	public class SheetsInteraction
	{
		internal class SheetsBenchmark
		{
			public string Title;
			public List<Task> Scenarios;
		}
		internal class Task
		{
			public string TaskName;
			public int Score;
			public string ScoreFieldPath;
		}

		private static readonly string[] Scopes = { SheetsService.Scope.Spreadsheets };
		private const string ApplicationName = "Revosect Spreadsheet Auto Updater";
		private static string SpreadsheetId = ""; //"12RKIvms4_22zgb1jVTh0OEH49n6YCrbz3Lxhj1NwZ6s"; //"1VxVS6VoGctS-EKCS2Hf5XeZGbHr97jzL1Uq78NQTmgo";
		private const string Sheet = "Hard"; //"Tabellenblatt1";
		private static SheetsService _service;
		static char[] columns = new char[] { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' };


		public static void Init()
		{
			GoogleCredential credential;
			using (var stream = new FileStream("spreadsheetupdater-351104-469141b0dd58.json", FileMode.Open, FileAccess.Read))
			{
				credential = GoogleCredential.FromStream(stream).CreateScoped(Scopes);
			}

			_service = new SheetsService(new BaseClientService.Initializer()
			{
				HttpClientInitializer = credential,
				ApplicationName = ApplicationName,
			});

			SpreadsheetId = MainWindow.viewModel.BenchmarkSpreadSheetId;
			//CreateMultipleEntries(5);
			//ReadEntries("A5:E5");

			//CreateEntry("A2:F2", new List<object>() { "Hello!", "This", "was", "inserted", "via", "C#" , "", "yo"});
			//Thread.Sleep(1000);
			//UpdateEntry("D2", new List<object>() { "updated" });
		}

		public static void updateSheetWithBenchmark(Benchmark bench)
        {
			var values = new List<object>();
			for (int i = 0; i < bench.Categories.Length; i++)
                for (int j = 0; j < bench.Categories[i].Subcategories.Length; j++)
                    for (int k = 0; k < bench.Categories[i].Subcategories[j].Scenarios.Length; k++)
						values.Add(bench.Categories[i].Subcategories[j].Scenarios[k].Score);

			var sheetResults = ReadEntries("H4:H21");
			for (int i = 0; i < 18; i++)
				if((int)values[i] > int.Parse((string)sheetResults[i][0]))
					UpdateEntry($"H{i + 4}", new List<object>() { values[i] });
		}

		private static SheetsBenchmark createBenchmark()
		{
			var values = ReadEntries("A:Q"); //<- reads entire column
			if (values != null)
			{
				var bench = new SheetsBenchmark() { Title = "Hard", Scenarios = new List<Task>() };

				for (int i = 0; i < values.Count; i++)
				{
					for (int j = 0; j < values[i].Count; j++)
					{
						if (j == 4 && values[i][j].ToString().Contains("rA"))
						{
							bench.Scenarios.Add(new Task()
							{
								TaskName = values[i][j].ToString(),
								Score = int.Parse(values[i][j + 3].ToString()),
								ScoreFieldPath = $"{columns[j + 3]}{i + 1}",
							});
							break;
						}
					}
				}

				return bench;

				/*if (twoshotscore < 1330)
				{
					UpdateEntry(twoshotScoresFieldAdress, new List<object> { "1337" });
				}*/
			}

			return null;
		}

		private static IList<IList<object>> ReadEntries(string fields)
		{
			var range = $"{Sheet}!{fields}";
			SpreadsheetsResource.ValuesResource.GetRequest request = _service.Spreadsheets.Values.Get(SpreadsheetId, range);
			var response = request.Execute();
			IList<IList<object>> values = response.Values;
			if (values != null && values.Count > 0)
			{
				return values;
			}
			else Console.WriteLine("No data found.");
			return null;
		}

		//if there already is content in those fields, apparently it tries to write the contents in the new row
		private static void CreateEntry(string fields, List<object> contents)
		{
			var range = $"{Sheet}!{fields}";
			var valueRange = new ValueRange();
			valueRange.Values = new List<IList<object>> { contents };

			var appendRequest = _service.Spreadsheets.Values.Append(valueRange, SpreadsheetId, range);
			appendRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
			appendRequest.Execute();
		}

		private static void UpdateEntry(string fields, List<object> contents)
		{
			var range = $"{Sheet}!{fields}";
			var valueRange = new ValueRange();
			valueRange.Values = new List<IList<object>> { contents };

			var updateRequest = _service.Spreadsheets.Values.Update(valueRange, SpreadsheetId, range);
			updateRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;
			updateRequest.Execute();
		}

		private static void DeleteEntry(string fields)
		{
			var range = $"{Sheet}!{fields}";
			var requestBody = new ClearValuesRequest();

			var deleteRequest = _service.Spreadsheets.Values.Clear(requestBody, SpreadsheetId, range);
			deleteRequest.Execute();
		}
	}
}
