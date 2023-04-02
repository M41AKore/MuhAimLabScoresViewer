using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MuhAimLabScoresViewer
{
    /// <summary>
    /// Interaction logic for LiveTrackerTab.xaml
    /// </summary>
    public partial class LiveTrackerTab : Page
    {
        public LiveTrackerTab()
        {
            InitializeComponent();
            DataContext = MainWindow.viewModel;
        }

        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            try
            {
                Logger.log("reading db file...");
                if (LiveTracker.sqlite == null) LiveTracker.sqlite = new SQLiteConnection($"Data Source={LiveTracker.LocalDBFile}"); //;New=False;

                var currentTask = LiveTracker.currentScenario;
                if (currentTask == null) return;

                var results = LiveTracker.selectQuery($"SELECT * FROM TaskData WHERE taskName = '{currentTask.Identification}'");
                var rows = results.Select();

                List<int> scores = new List<int>();
                foreach (var row in rows)
                    scores.Add(int.Parse(row["score"].ToString()));

                //highscore
                int max = scores.Max();
                double diff = double.Parse(txt_TrackerHighscore.Text) / max;
                var docky = new DockPanel();
                docky.Children.Add(new TextBlock() { Text = "Highscore: " });
                docky.Children.Add(new TextBlock()
                {
                    Text = $"{getDiffString(diff)}%",
                    Foreground = diff < 1 ? Brushes.Red : Brushes.Green,
                });
                historicComparisonStacky.Children.Add(docky);

                //average
                double average = scores.Sum() / scores.Count;
                diff = double.Parse(txt_TrackerAverage.Text) / average;
                docky = new DockPanel();
                docky.Children.Add(new TextBlock() { Text = "Average: " });
                docky.Children.Add(new TextBlock()
                {
                    Text = $"{getDiffString(diff)}%",
                    Foreground = diff < 1 ? Brushes.Red : Brushes.Green,
                });
                historicComparisonStacky.Children.Add(docky);


                //median
                var sortedScores = scores.OrderByDescending(s => s).ToList();
                double median = 0;
                if (sortedScores.Count % 2 == 0) //if even, calculate median at half
                {
                    int halfPlusOne = (sortedScores.Count) / 2;
                    int halfMinusOne = halfPlusOne - 1;
                    median = (sortedScores[halfPlusOne] + sortedScores[halfMinusOne]) / 2;
                }
                else median = sortedScores[sortedScores.Count / 2]; //median is at half

                diff = double.Parse(txt_TrackerMedian.Text) / median;
                docky = new DockPanel();
                docky.Children.Add(new TextBlock() { Text = "Median: " });
                docky.Children.Add(new TextBlock()
                {
                    Text = $"{getDiffString(diff)}%",
                    Foreground = diff < 1 ? Brushes.Red : Brushes.Green,
                });
                historicComparisonStacky.Children.Add(docky);
            }
            catch (Exception ex)
            {
                Logger.log("exception thrown when trying to read database file!" + Environment.NewLine + ex.Message);
            }
        }
        private string getDiffString(double ratio) => ratio < 1 ? "-" + ((1 - ratio) * 100).ToString("0.##") : "+" + ((ratio - 1) * 100).ToString("0.##");
    }
}
