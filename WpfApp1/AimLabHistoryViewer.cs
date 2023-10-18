using Newtonsoft.Json;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot.Wpf;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using static MuhAimLabScoresViewer.APIStuff.AGGProfileResult.AimLab.PlayAGG.Aggregate;
using static MuhAimLabScoresViewer.Helper;
using static MuhAimLabScoresViewer.ObjectsAndStructs;

namespace MuhAimLabScoresViewer
{
    public class AimLabHistoryViewer
    {
        public static List<ScenarioHistory> Scenarios;
        public static ScenarioHistory currentScenario;

        public static AimLabHistory? getData(string filepath)
        {
            var filecontent = File.ReadAllText(filepath);
            if (filecontent.StartsWith("[["))
            {
                filecontent = filecontent.Substring(1, filecontent.Length - 2);
                if (filecontent.EndsWith("]]"))
                {
                    filecontent = filecontent.Substring(0, filecontent.Length - 2);
                }
            }
            filecontent = "{ \"historyEntries\":" + filecontent + "}";

            try
            {
                var item = JsonConvert.DeserializeObject<AimLabHistory>(filecontent);
                if (item == null) MessageBox.Show("deserialization failed!");

                return item;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            return null;
        }

        public static void sortTasks(HistoryEntry1[] historyEntries)
        {
            Scenarios = new List<ScenarioHistory>();

            foreach (var historyEntry in historyEntries)
            {
                if(historyEntries == null) continue;

                var existing = Scenarios.FirstOrDefault(s => s.Identification == historyEntry.taskName);
                if (existing == null)
                {
                    existing = new ScenarioHistory()
                    {
                        Identification = historyEntry.taskName,
                        Name = getTaskNameFromLevelID(historyEntry.taskName, historyEntry.workshopId), 
                        Plays = new List<Play>(),
                    };
                    if(string.IsNullOrEmpty(existing.Name)) existing.Name = existing.Identification;
                    Scenarios.Add(existing);
                }

                existing.Plays.Add(new Play()
                {
                    DateString = historyEntry.create_date,
                    Date = DateTime.Parse(historyEntry.create_date),
                    Score = historyEntry.score,
                    Accuracy = "", //parse performance item for this
                });
            }

            Scenarios.ForEach(s => s.Plays = s.Plays.OrderBy(p => p.Date).ToList()); //make sure they're in timely order
            Scenarios = Scenarios.OrderBy(s => s.Name).ToList(); //order alphabetically

            createScenariosGUI(MainWindow.viewModel.SortType.Name, MainWindow.viewModel.SortDirection.Name);
        }
    
        public static void createScenariosGUI(string sortType = "Name", string sortDirection = "Ascending")
        {
            var tab = (MainWindow.Instance.windowTabs[3] as AimLabHistoryTab);
            if (tab == null) return;
            tab.scenariosStacky.Children.Clear();

            switch (sortType)
            {
                case "Name": //order alphabetically
                    if (sortDirection == "Descending") Scenarios = Scenarios.OrderByDescending(s => s.Name).ToList();
                    else Scenarios = Scenarios.OrderBy(s => s.Name).ToList();
                    break;
                case "Plays": //order by playcount
                    if (sortDirection == "Descending") Scenarios = Scenarios.OrderByDescending(s => s.Plays.Count).ToList();
                    else Scenarios = Scenarios.OrderBy(s => s.Plays.Count).ToList();
                    break;
                case "Date": //order by last played
                    if (sortDirection == "Descending") Scenarios = Scenarios.OrderByDescending(s => s.Plays.FirstOrDefault()?.Date).ToList();
                    else Scenarios = Scenarios.OrderBy(s => s.Plays.FirstOrDefault()?.Date).ToList();
                    break;
                default:
                    break;
            }

            for (int i = 0; i < Scenarios.Count; i++)
            {
                var btn = new Button()
                {
                    Name = $"HistoryScenarioButton_{i}",
                    Content = Scenarios[i].Name,
                    Width = 240,
                    Height = 20,
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
                    HorizontalContentAlignment = System.Windows.HorizontalAlignment.Left,
                };
                btn.Click += Btn_Click;
                tab.scenariosStacky.Children.Add(btn);
            }
        }
        
        public static void Btn_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn != null)
            {
                int i = int.Parse(btn.Name.Split('_')[1]);
                ((AimLabHistoryTab)MainWindow.Instance.windowTabs[3]).GraphTitle.Content = !string.IsNullOrEmpty(Scenarios[i].Name) ? Scenarios[i].Name : Scenarios[i].Identification;
                createDataPoints(Scenarios[i], MainWindow.viewModel.GraphPlayDisplayCount);
            }
        }

        public static void createDataPoints(ScenarioHistory scenario, string playCount = "all")
        {
            currentScenario = scenario;
            if (currentScenario == null) return;

            PlotView pv = new PlotView();
            pv.Height = 600;
            pv.Width = 600;

            /*DateTimeAxis dateTimeAxis = new DateTimeAxis()
            {
                Position = AxisPosition.Bottom,
                StringFormat = "dd/MM/yyyy",
                Title = "Time",
                MinorIntervalType = DateTimeIntervalType.Days,
                IntervalType = DateTimeIntervalType.Days,
                MajorGridlineStyle = OxyPlot.LineStyle.Solid,
                MinorGridlineStyle = OxyPlot.LineStyle.None,
            };*/

            //scores plot
            FunctionSeries fs = new FunctionSeries();
            //fs.TrackerFormatString = "Time: {2:dd.MM.yyyy} X: {4:0.###}";

            int count = scenario.Plays.Count;
            switch(playCount)
            {
                case "last 50":
                    count = 50;
                    break;
                case "last 100":
                    count = 100;
                    break;
                case "last 200":
                    count = 200;
                    break;
                case "last 500":
                    count = 500;
                    break;
                case "last 1000":
                    count = 1000;
                    break;
                default: //inlc "all", ^ Plays.Count
                    break;
            }
            var plays = count < scenario.Plays.Count ? scenario.Plays.TakeLast(count).ToList() : scenario.Plays;

            for (int i = 0; i < plays.Count; i++)
            {
                //var playDate = DateTime.Parse(scenario.Plays[i].Date);
                fs.Points.Add(new OxyPlot.DataPoint(i + 1, int.Parse(plays[i].Score))); //DateTimeAxis.ToDouble(playDate)
            }

            //caluclate medians
            var medians = new List<double>();
            for (int i = 0; i < plays.Count; i++)
            {
                var group = plays.Take(i + 1).OrderBy(p => int.Parse(p.Score)).ToArray();
                if ((i + 1) % 2 == 0) //if even, calculate median at half
                {
                    int halfPlusOne = (i + 1) / 2; // f.e. 12 / 2 = 6, which would be top of bottom half, but since it's index it's 7th, aka first of top half
                    int halfMinusOne = halfPlusOne - 1; // ^the actual 6th number, aka top of bottom half
                    medians.Add((int.Parse(group[halfPlusOne].Score) + int.Parse(group[halfMinusOne].Score)) / 2);
                }
                else medians.Add(double.Parse(group[(i + 1) / 2].Score)); //if uneven, median is at half
            }

            //median plot
            FunctionSeries fs2 = new FunctionSeries();
            //fs2.TrackerFormatString = "Time: {2:dd.MM.yyyy} X: {4:0.###}";

            for (int i = 0; i < medians.Count; i++)
                fs2.Points.Add(new OxyPlot.DataPoint(i + 1, medians[i]));

            fs2.Color = OxyColor.FromArgb(255, 255, 0, 0);

            PlotModel n = new PlotModel();
            n.Series.Add(fs);
            n.Series.Add(fs2);

            //n.Axes.Add(dateTimeAxis);
            n.Axes.Add(new LinearAxis()
            {
                Position = AxisPosition.Bottom,
                Title = "Plays",
                MajorGridlineStyle = OxyPlot.LineStyle.Solid,
                MinorGridlineStyle = OxyPlot.LineStyle.None,
            });

            n.Axes.Add(new LinearAxis());
            pv.Model = n;

            var tab = MainWindow.Instance.windowTabs[3] as AimLabHistoryTab;
            if (tab == null) return;
            if (tab.graphStacky.Children.Count > 0) tab.graphStacky.Children.RemoveAt(0);
            tab.graphStacky.Children.Add(pv);

            //other info section
            var orderedPlays = plays.OrderBy(p => int.Parse(p.Score)).ToArray();

            tab.txt_Plays.Text = plays.Count.ToString();
            tab.txt_Highscore.Text = orderedPlays.Last().Score.ToString();
            tab.txt_Average.Text = plays.Average(p => int.Parse(p.Score)).ToString("#.##");

            //median
            if (plays.Count % 2 == 0) //if even, calculate median at half
            {
                int halfPlusOne = int.Parse(orderedPlays[plays.Count / 2].Score);
                int halfMinusOne = int.Parse(orderedPlays[(plays.Count / 2) - 1].Score);
                double median = ((double)(halfPlusOne + halfMinusOne) / 2);
                tab.txt_Median.Text = median.ToString();
            }
            else tab.txt_Median.Text = orderedPlays[plays.Count / 2].Score.ToString();

            tab.txt_MinScore.Text = orderedPlays.First()?.Score;


            tab.txt_Hits.Text = orderedPlays.First().Hits;
            tab.txt_Misses.Text = orderedPlays.First().Misses;
        }

        public static void pullDataFromLocalDB()
        {
            try
            {
                Logger.log("reading db file...");
                if(LiveTracker.sqlite == null) LiveTracker.sqlite = new SQLiteConnection($"Data Source={LiveTracker.LocalDBFile}"); //;New=False;

                var results = LiveTracker.selectQuery("SELECT * FROM TaskData ORDER BY taskName DESC");
                var rows = results.Select();

                Scenarios = new List<ScenarioHistory>();

                foreach (var row in rows)
                {
                    string? taskname = row["taskName"].ToString();
                    if (taskname == null) continue;

                    var existing = Scenarios.FirstOrDefault(s => s.Identification == taskname);
                    if (existing == null)
                    {
                        string scenName = null;
                        try
                        {
                            scenName = getTaskNameFromLevelID(taskname, row["workshopId"].ToString());
                            
                        }
                        catch(Exception ex)
                        {
                            Trace.WriteLine(ex.Message);
                        }

                        existing = new ScenarioHistory()
                        {
                            Identification = taskname,
                            Name = scenName,
                            Plays = new List<Play>(),
                        };
                        if (string.IsNullOrEmpty(existing.Name)) existing.Name = existing.Identification;
                        Scenarios.Add(existing);

                        Trace.WriteLine("added " + scenName);
                    }

                    string? createDate = row["createDate"].ToString();

                    var perfString = row["performance"].ToString();
                    var parts = perfString.Replace('\\', ' ').Split(',');

                    string? acc = null;
                    var value = parts.FirstOrDefault(p => p.Contains("accTotal"));
                    if(value != null)
                    {
                        var p = value.Split(':');
                        if(p != null && p.Any()) acc = p[1];
                    }

                    string? kills = null;
                    /*value = parts.FirstOrDefault(p => p.Contains("killTotal"));
                    if (value != null)
                    {
                        var p = value.Split(':');
                        if (p != null && p.Any()) kills = p[1].Replace('}', ' ').Trim();
                    }*/

                    string? shots = null;
                    /*value = parts.FirstOrDefault(p => p.Contains("shotsTotal"));
                    if (value != null)
                    {
                        var p = value.Split(':');
                        if (p != null && p.Any()) shots = p[1];
                    }*/

                    string? targets = null;
                    /*value = parts.FirstOrDefault(p => p.Contains("targetsTotal"));
                    if (value != null)
                    {
                        var p = value.Split(':');
                        if (p != null && p.Any()) targets = p[1];
                    }*/

                    string hits = shots != null && acc != null ? ((int)(int.Parse(shots) * (float.Parse(acc) * 0.01f))).ToString() : "0";

                    existing.Plays.Add(new Play()
                    {
                        DateString = createDate,
                        Date = createDate == null ? DateTime.MinValue : DateTime.Parse(createDate),
                        Score = row["score"].ToString(),
                        Accuracy = acc ?? "0",
                        Hits = hits,
                        Shots = shots ?? "0",
                        Kills = kills ?? "0",
                        Targets = targets ?? "0",
                        Misses = shots == null ? "0" : (int.Parse(shots) - int.Parse(hits)).ToString(),
                    });
                }

                Scenarios.ForEach(s => s.Plays = s.Plays.OrderBy(p => p.Date).ToList()); //make sure they're in timely order

                createScenariosGUI(MainWindow.viewModel.SortType.Name, MainWindow.viewModel.SortDirection.Name);
            }
            catch (Exception ex)
            {
                Logger.log("exception thrown when trying to read database file!" + Environment.NewLine + ex.Message);
            }
        }
        public static async Task<List<ScenarioHistory>> pullDataFromLocalDB2()
        {
            try
            {
                Logger.log("reading db file...");
                if (LiveTracker.sqlite == null) LiveTracker.sqlite = new SQLiteConnection($"Data Source={LiveTracker.LocalDBFile}"); //;New=False;

                var results = LiveTracker.selectQuery("SELECT * FROM TaskData ORDER BY taskName DESC");
                var rows = results.Select();

                var scenarios = new ConcurrentBag<ScenarioHistory>();
                Console.WriteLine($"found {rows.Length} local db entries!");

                var scenarioGroups = rows.GroupBy(r => r["taskName"].ToString());
                Console.WriteLine($"found {scenarioGroups.Count()} scenario groups!");

                var timestamp = DateTime.Now;
                var lookUp = await getTaskNamesFromLevelIDs();
                Console.WriteLine("getting tasklevels and ids took " + (DateTime.Now - timestamp).TotalMilliseconds + "ms!");

                await Task.Run(() => Parallel.ForEach(scenarioGroups, group =>
                {
                    lookUp.TryGetValue(group.Key, out string taskName);
                    var baseEntry = new ScenarioHistory()
                    {
                        Identification = group.Key,
                        Name = taskName,
                        Plays = new List<Play>(),
                    };
                    if (string.IsNullOrEmpty(baseEntry.Name)) baseEntry.Name = baseEntry.Identification;
                    scenarios.Add(baseEntry);

                    foreach (var row in group)
                    {
                        string? createDate = row["createDate"].ToString();

                        var perfString = row["performance"].ToString();
                        var parts = perfString.Replace('\\', ' ').Split(',');

                        string? acc = null;
                        var value = parts.FirstOrDefault(p => p.Contains("accTotal"));
                        if (value != null)
                        {
                            var p = value.Split(':');
                            if (p != null && p.Length > 1) acc = p[1];
                        }

                        string? kills = null;
                        value = parts.FirstOrDefault(p => p.Contains("killTotal"));
                        if (value != null)
                        {
                            var p = value.Split(':');
                            if (p != null && p.Length > 1) kills = p[1].Replace('}', ' ').Trim();
                        }

                        string? shots = null;
                        value = parts.FirstOrDefault(p => p.Contains("shotsTotal"));
                        if (value != null)
                        {
                            var p = value.Split(':');
                            if (p != null && p.Length > 1) shots = p[1];
                        }

                        string? targets = null;
                        value = parts.FirstOrDefault(p => p.Contains("targetsTotal"));
                        if (value != null)
                        {
                            var p = value.Split(':');
                            if (p != null && p.Length > 1) targets = p[1];
                        }

                        string hits = shots != null && acc != null ? ((int)(int.Parse(shots) * (float.Parse(acc) * 0.01f))).ToString() : "0";

                        baseEntry.Plays.Add(new Play()
                        {
                            DateString = createDate,
                            Date = createDate == null ? DateTime.MinValue : DateTime.Parse(createDate),
                            Score = row["score"].ToString(),
                            Accuracy = acc ?? "0",
                        });
                    }
                }));

                Console.WriteLine("grouping and creating base items took " + (DateTime.Now - timestamp).TotalMilliseconds + "ms!");
                var e = scenarios.ToList();
                e.ForEach(s => s.Plays = s.Plays.OrderBy(p => p.Date).ToList()); //make sure they're in timely order

                Scenarios = e;
                createScenariosGUI(MainWindow.viewModel.SortType.Name, MainWindow.viewModel.SortDirection.Name);
                return e;
            }
            catch (Exception ex)
            {
                Console.WriteLine("exception thrown when trying to read database file!" + Environment.NewLine + ex.Message);
                return null;
            }
            return null;
        }

        public static async Task<Dictionary<string, string>>? getTaskNamesFromLevelIDs()
        {
            if (!Directory.Exists(SettingsTab.currentSettings.SteamLibraryPath)) return null;

            var dictionary = new Dictionary<string, string>();

            foreach (var dir in new DirectoryInfo(SettingsTab.currentSettings.SteamLibraryPath + @"\steamapps\workshop\content\714010").GetDirectories())
            {
                foreach (var subdir in dir.GetDirectories())
                    if (subdir.Name == "Levels")
                        foreach (var file in subdir.GetDirectories()[0].GetFiles())
                            if (file.Name == "level.es3")
                            {
                                var content = File.ReadAllText(file.FullName);
                                string foundLevelId = getLevelIDFromES3(content);
                                string taskName = getTaskNameFromES3(content);

                                if (!dictionary.ContainsKey(foundLevelId) && !dictionary.ContainsValue(taskName))
                                {
                                    dictionary.Add(foundLevelId, taskName);
                                }

                            }
            }

            return dictionary;
        }
    }  
}
