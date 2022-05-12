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
using static MuhAimLabScoresViewer.Helper;
using static MuhAimLabScoresViewer.ObjectsAndStructs;

namespace MuhAimLabScoresViewer
{
    public class AimLabHistoryViewer
    {
        private static List<ScenarioHistory> Scenarios;

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

            createScenariosGUI();
        }
    
        private static void createScenariosGUI()
        {
            MainWindow.Instance.scenariosStacky.Children.Clear();

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
            var orderedPlays = scenario.Plays.OrderBy(p => int.Parse(p.Score)).ToArray();

            MainWindow.Instance.txt_Plays.Text = scenario.Plays.Count.ToString();
            MainWindow.Instance.txt_Highscore.Text = orderedPlays.Last().Score.ToString();
            MainWindow.Instance.txt_Average.Text = scenario.Plays.Average(p => int.Parse(p.Score)).ToString("#.##");

            //median
            if (scenario.Plays.Count % 2 == 0) //if even, calculate median at half
            {
                int halfPlusOne = int.Parse(orderedPlays[scenario.Plays.Count / 2].Score);
                int halfMinusOne = int.Parse(orderedPlays[(scenario.Plays.Count / 2) - 1].Score);
                double median = ((double)(halfPlusOne + halfMinusOne) / 2);
                MainWindow.Instance.txt_Median.Text = median.ToString();
            }
            else
            {
                MainWindow.Instance.txt_Median.Text = orderedPlays[scenario.Plays.Count / 2].Score.ToString();
            }
        }  
    }  
}
