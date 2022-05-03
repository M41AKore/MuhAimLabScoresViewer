using Newtonsoft.Json;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot.Wpf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MuhAimLabScoresViewer
{
    public class AimLabHistoryViewer
    {
        private static List<ScenarioHistory> Scenarios;

        public static void sortTasks(HistoryEntry[] historyEntries)
        {
            Scenarios = new List<ScenarioHistory>();

            foreach (var historyEntry in historyEntries)
            {
                var existing = Scenarios.FirstOrDefault(s => s.Identification == historyEntry.taskName);
                if (existing == null)
                {
                    existing = new ScenarioHistory()
                    {
                        Identification = historyEntry.taskName,
                        Name = getTaskNameFromLevelID(historyEntry.taskName, historyEntry.workshopId), 
                        Plays = new List<Play>(),
                    };
                    Scenarios.Add(existing);
                }

                existing.Plays.Add(new Play()
                {
                    Date = historyEntry.create_date,
                    Score = historyEntry.score,
                    Accuracy = "", //parse performance item for this
                });
            }

            createScenariosGUI();
        }

        public static string getTaskNameFromLevelID(string levelid, string workshopid)
        {
            if (!Directory.Exists(MainWindow.currentSettings.SteamLibraryPath)) return null;

            DirectoryInfo[] dirs = new DirectoryInfo(MainWindow.currentSettings.SteamLibraryPath + @"\steamapps\workshop\content\714010").GetDirectories();
            foreach (var dir in dirs)
            {
                if (!string.IsNullOrEmpty(workshopid) && dir.Name != workshopid) continue;

                foreach (var subdir in dir.GetDirectories())
                    if (subdir.Name == "Levels")
                        foreach (var file in subdir.GetDirectories()[0].GetFiles())
                            if (file.Name == "level.es3")
                            {
                                var content = File.ReadAllText(file.FullName);
                                if (content.Contains(levelid)) return getTaskNameFromES3(content);
                            }
            }
                

            return null;
        }
        private static string getTaskNameFromES3(string filecontent)
        {
            var start = filecontent.IndexOf("contentMetadata");
            var relevant = filecontent.Substring(start, filecontent.IndexOf("category") - start);
            var lines = relevant.Split(new string[] { "\",", "{", "}" }, StringSplitOptions.RemoveEmptyEntries);
            var relevantline = lines.FirstOrDefault(l => l.Contains("label"));      
            var result = uglyCleanup(relevantline);
            return result;
        }
        private static string uglyCleanup(string s)
        {
            s = s.Substring(s.IndexOf(':'), s.Length - s.IndexOf(':'));
            s = s.Trim(new char[] { ':', '\\', '"' });
            s = s.Trim('"');
            s = s.Replace('"', ' ');
            s = s.Trim();
            //seems to do it...
            return s;
        }


        private static void createScenariosGUI()
        {
            for (int i = 0; i < Scenarios.Count; i++)
            {
                var btn = new Button()
                {
                    Name = $"HistoryScenarioButton_{i}",
                    Content = !string.IsNullOrEmpty(Scenarios[i].Name) ? Scenarios[i].Name : Scenarios[i].Identification,
                    Width = 240,
                    Height = 20,
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
                    HorizontalContentAlignment = System.Windows.HorizontalAlignment.Left,
                };
                btn.Click += Btn_Click;
                MainWindow.Instance.scenariosStacky.Children.Add(btn);
            }
        }
        public static void Btn_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn != null)
            {
                int i = int.Parse(btn.Name.Split('_')[1]);
                MainWindow.Instance.GraphTitle.Content = !string.IsNullOrEmpty(Scenarios[i].Name) ? Scenarios[i].Name : Scenarios[i].Identification;
                createDataPoints(Scenarios[i]);
            }
        }

        private static void createDataPoints(ScenarioHistory scenario)
        {
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

            for (int i = 0; i < scenario.Plays.Count; i++)
            {
                //var playDate = DateTime.Parse(scenario.Plays[i].Date);
                fs.Points.Add(new OxyPlot.DataPoint(i + 1, int.Parse(scenario.Plays[i].Score))); //DateTimeAxis.ToDouble(playDate)

            }

            //caluclate medians
            var medians = new List<double>();
            for (int i = 0; i < scenario.Plays.Count; i++)
            {
                var group = scenario.Plays.Take(i + 1).OrderBy(p => int.Parse(p.Score)).ToArray();
                if ((i + 1) % 2 == 0) //if even, calculate median at half
                {
                    int halfPlusOne = (i + 1) / 2;
                    int halfMinusOne = halfPlusOne - 1;

                    medians.Add((int.Parse(group[halfPlusOne].Score) + int.Parse(group[halfMinusOne].Score)) / 2);
                }
                else
                {
                    medians.Add(double.Parse(group[(i + 1) / 2].Score));
                }
            }

            //median plot
            FunctionSeries fs2 = new FunctionSeries();
            //fs2.TrackerFormatString = "Time: {2:dd.MM.yyyy} X: {4:0.###}";

            for (int i = 0; i < medians.Count; i++)
            {
                fs2.Points.Add(new OxyPlot.DataPoint(i + 1, medians[i]));
            }

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

            if (MainWindow.Instance.graphStacky.Children.Count > 0) MainWindow.Instance.graphStacky.Children.RemoveAt(0);
            MainWindow.Instance.graphStacky.Children.Add(pv);

            //other info section
            var orderedPlays = scenario.Plays.OrderByDescending(p => int.Parse(p.Score)).ToArray();

            MainWindow.Instance.txt_Plays.Text = scenario.Plays.Count.ToString();
            MainWindow.Instance.txt_Highscore.Text = orderedPlays[0].Score.ToString();
            MainWindow.Instance.txt_Average.Text = scenario.Plays.Average(p => int.Parse(p.Score)).ToString("#.##");

            //median
            if (scenario.Plays.Count % 2 == 0) //if even, calculate median at half
            {
                int halfPlusOne = scenario.Plays.Count / 2;
                int halfMinusOne = halfPlusOne - 1;
                double median = halfPlusOne + halfMinusOne / 2;
                MainWindow.Instance.txt_Median.Text = median.ToString();
            }
            else
            {
                MainWindow.Instance.txt_Median.Text = orderedPlays[scenario.Plays.Count / 2].Score.ToString();
            }
        }

        public static AimLabHistory getData(string filepath)
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
                if (item == null)
                {
                    MessageBox.Show("deserialization failed!");
                }
                return item;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            return null;
        }
    }

    public class ScenarioHistory
    {
        public string Identification;
        public string Name;
        public List<Play> Plays;
    }

    public class Play
    {
        public string Date;
        public string Score;
        public string Accuracy;
    }

    public class Holder
    {
        public double X { get; set; }
        public double Y { get; set; }
        public Point Point { get; set; }

        public Holder()
        {
        }
    }

    public class Value
    {
        public double X { get; set; }
        public double Y { get; set; }

        public Value(double x, double y)
        {
            X = x;
            Y = y;
        }
    }
}
