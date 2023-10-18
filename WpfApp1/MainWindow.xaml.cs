using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using System.Drawing;
using MessageBox = System.Windows.MessageBox;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using Brushes = System.Windows.Media.Brushes;
using Button = System.Windows.Controls.Button;
using static MuhAimLabScoresViewer.Helper;
using static MuhAimLabScoresViewer.ObjectsAndStructs;
//using static OBS.libobs;
using System.Data.SQLite;
using System.Collections.Concurrent;

namespace MuhAimLabScoresViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow Instance { get; private set; }
        public static ViewModel viewModel;
        public static bool windowActivated = false;
        public static CustomMessageBox currentMsgBox = null;
        public List<Page> windowTabs = new List<Page>();
        public List<System.Windows.Shapes.Rectangle> tabButtonBottomBorders = new List<System.Windows.Shapes.Rectangle>();

        public MainWindow()
        {
            if (Instance == null) Instance = this;
            else
            {
                showMessageBox("there's already an instance running!");
                this.Close();
            }

            InitializeComponent();
            this.DataContext = new ViewModel();
            viewModel = this.DataContext as ViewModel;

            initPages();

            Logger.setup();
            string LocalLowPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).Replace("Roaming", "LocalLow");
            LiveTracker.LocalDBFile = Path.Combine(LocalLowPath, "Statespace\\aimlab_tb\\klutch.bytes");

            Settings.loadSettings();

            if (viewModel.LiveTrackerEnabled)
            {
                LiveTracker.createLiveTrackerGUI();
                LiveTracker.generateResultView(windowTabs[5] as LiveResultTab);
            }
        }

        private void initPages()
        {
            windowTabs.Add(new TasksTab() { Visibility = Visibility.Visible });
            windowTabs.Add(new BenchmarkTab() { Visibility = Visibility.Collapsed });
            windowTabs.Add(new CompetitionTab() { Visibility = Visibility.Collapsed });
            windowTabs.Add(new AimLabHistoryTab() { Visibility = Visibility.Collapsed });
            windowTabs.Add(new LiveTrackerTab() { Visibility = Visibility.Collapsed });
            windowTabs.Add(new LiveResultTab() { Visibility = Visibility.Collapsed });
            windowTabs.Add(new SettingsTab() { Title = "Settings", Visibility = Visibility.Collapsed });

            tabButtonBottomBorders.Add(TaskButton_BottomBorder);
            tabButtonBottomBorders.Add(BenchmarkButton_BottomBorder);
            tabButtonBottomBorders.Add(CompetitionButton_BottomBorder);
            tabButtonBottomBorders.Add(AimLabHistoryButton_BottomBorder);
            tabButtonBottomBorders.Add(LiveTrackerButton_BottomBorder);
            tabButtonBottomBorders.Add(LiveResultsButton_BottomBorder);
            tabButtonBottomBorders.Add(SettingsButton_BottomBorder);

            changeToTab(0);
        }
        private void changeToTab(int tabIndex)
        {
            tabsContentFrame.Content = windowTabs[tabIndex];
            windowTabs[tabIndex].Visibility = Visibility.Visible;
            windowTabs.Where((t, i) => i != tabIndex).ToList().ForEach(t => t.Visibility = Visibility.Collapsed);

            tabButtonBottomBorders[tabIndex].Visibility = Visibility.Hidden;
            tabButtonBottomBorders.Where((t, i) => i != tabIndex).ToList().ForEach(t => t.Visibility = Visibility.Visible);
        }
        private void TaskButton_Click(object sender, RoutedEventArgs e)
        {
            changeToTab(0);
            this.Height = 525;
            this.Width = 700;
        }
        private void BenchmarkButton_Click(object sender, RoutedEventArgs e)
        {
            changeToTab(1);
            this.Height = 840;
            this.Width = 800;
        }
        private void CompetitionButton_Click(object sender, RoutedEventArgs e)
        {
            changeToTab(2);
            this.Height = 580;
            this.Width = 1060;
        }
        private void AimLabHistoryButton_Click(object sender, RoutedEventArgs e)
        {
            changeToTab(3);
            this.Height = 750;
            this.Width = 1050;
        }
        private void LiveTrackerButton_Click(object sender, RoutedEventArgs e)
        {
            changeToTab(4);
            this.Height = 750;
            this.Width = 800;
        }
        private void LiveResultButton_click(object sender, RoutedEventArgs e)
        {
            changeToTab(5);
            this.Height = 750;
            this.Width = 800;
        }
        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            changeToTab(6);
            this.Height = 700;
            this.Width = 700;
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var part = btn.Name.Substring(11, btn.Name.Length - 11);
            string link = generateLink(part); // $"playbutton_{parts[0]}_{parts[1]}",   0 = authorid, 1 = workshopid

            ProcessStartInfo processStartInfo = new ProcessStartInfo(link);
            processStartInfo.UseShellExecute = true;
            System.Diagnostics.Process.Start(processStartInfo);
        }
        
        private void OnActivated(object sender, EventArgs eventArgs)
        {
            windowActivated = true;
            Activated -= OnActivated;
        }
        private void Window_Closed(object sender, EventArgs e)
        {
            Settings.SaveSettings();
        }

        public MessageBoxResult showMessageBox(string text, MessageBoxButtons layout = MessageBoxButtons.OK)
        {
            MessageBoxResult result = MessageBoxResult.None;

            this.Dispatcher.Invoke(() =>
            {
                var msgBox = new CustomMessageBox();
                msgBox.setText(text);

                //customize layouut depending on layout param

                //if messages would have to be displayed while the mainwindow is not properly loaded yet, use default box
                //else the "Owner" property causes exception
                if (MainWindow.windowActivated)
                {
                    if (currentMsgBox != null)
                    {
                        currentMsgBox.Close();
                        currentMsgBox = null;
                    }

                    msgBox.Owner = MainWindow.Instance;
                    msgBox.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    msgBox.ShowDialog();
                    currentMsgBox = msgBox;
                }
                else MessageBox.Show(text);
            });

            //add it returning msgboxresult

            return result;
        }
        public void displayAutoRecordStatusMessage(string s)
        {
            Task.Run(() =>
            {
                this.Dispatcher.Invoke(() => autoRecordStatus_Output.Text = s);
                Thread.Sleep(3000);
                this.Dispatcher.Invoke(() => autoRecordStatus_Output.Text = "Listening...");
            });
        }




        //inactive benchmark leaderboard view
        private void BenchmarkLeaderboardButton_Click(object sender, RoutedEventArgs e)
        {
            //windowTabs[6]
            this.Height = 600;
            this.Width = 1200;
        }
        public class BenchmarkLeaderboard
        {
            public string TaskName { get; set; }
            public Item ResultsItem { get; set; }
        }
        public ConcurrentBag<BenchmarkLeaderboard> benchmarkLeaderboards = new ConcurrentBag<BenchmarkLeaderboard>();
        public class BenchmarkLeaderboardContender
        {
            public string klutchId { get; set; }
            public string UserName { get; set; }
            public Benchmark Benchmark { get; set; }
            public DateTime NewestScoreTimestamp { get; set; }
        }
        public class TaskScore
        {
            public string TaskName { get; set; }
            public int Score { get; set; }
            public float Energy { get; set; }
        }
        public List<BenchmarkLeaderboardContender> BenchmarkLeaderboardContenders = new List<BenchmarkLeaderboardContender>();
        private void createBenchmarkLeaderboard(Benchmark bench)
        {
            List<Task> tasks = new List<Task>();
            var leaderboards = new List<Item>();

            Trace.WriteLine(benchmarkLeaderboards.Count);

            for (int i = 0; i < bench.Categories.Length; i++)
                for (int j = 0; j < bench.Categories[i].Subcategories.Length; j++)
                    for (int k = 0; k < bench.Categories[i].Subcategories[j].Scenarios.Length; k++)
                    {
                        var board = benchmarkLeaderboards.FirstOrDefault(b => b.TaskName == bench.Categories[i].Subcategories[j].Scenarios[k].Name);

                        BenchmarkLeaderboard? alternative = null;
                        if (!string.IsNullOrEmpty(bench.Categories[i].Subcategories[j].Scenarios[k].AlternativeName))
                            alternative = benchmarkLeaderboards.FirstOrDefault(b => b.TaskName == bench.Categories[i].Subcategories[j].Scenarios[k].AlternativeName);

                        if (board != null)
                        {
                            foreach (var result in board.ResultsItem.results)
                            {
                                var existing = BenchmarkLeaderboardContenders.FirstOrDefault(c => c.klutchId == result.klutchId);
                                if (existing == null)
                                {
                                    existing = new BenchmarkLeaderboardContender()
                                    {
                                        klutchId = result.klutchId,
                                        UserName = result.username,
                                        Benchmark = XmlSerializer.deserializeXml<Benchmark>(viewModel.LastBenchmarkPath),
                                        NewestScoreTimestamp = UnixTimeStampToDateTime(double.Parse(result.endedAt.Substring(0, result.endedAt.Length - 3))),
                                    };
                                    BenchmarkLeaderboardContenders.Add(existing);
                                }

                                if (alternative != null)
                                {
                                    var alternativeScore = alternative.ResultsItem.results.FirstOrDefault(a => a.klutchId == existing.klutchId)?.score;
                                    if (alternativeScore != null && alternativeScore > 0 && result.score < alternativeScore)
                                        existing.Benchmark.Categories[i].Subcategories[j].Scenarios[k].Score = (int)alternativeScore;
                                    else
                                        existing.Benchmark.Categories[i].Subcategories[j].Scenarios[k].Score = result.score;
                                }
                                else existing.Benchmark.Categories[i].Subcategories[j].Scenarios[k].Score = result.score;

                                if (existing.UserName != result.username)
                                {
                                    var resultTimestamp = UnixTimeStampToDateTime(double.Parse(result.endedAt.Substring(0, result.endedAt.Length - 3)));
                                    if (resultTimestamp > existing.NewestScoreTimestamp)
                                    {
                                        Trace.WriteLine(resultTimestamp + " is more recent than " + existing.NewestScoreTimestamp);
                                        existing.UserName = result.username;
                                        existing.NewestScoreTimestamp = resultTimestamp;
                                    }
                                }

                                existing.Benchmark.Categories[i].Subcategories[j].Scenarios[k].calculateEnergy(existing.Benchmark.Categories[i].Subcategories[j].Scenarios[k].Score);
                            }
                        }
                    }

            foreach (var contender in BenchmarkLeaderboardContenders)
            {
                foreach (var cat in contender.Benchmark.Categories)
                {
                    foreach (var subcat in cat.Subcategories)
                    {
                        foreach (var scen in subcat.Scenarios)
                        {
                            contender.Benchmark.EnergyPerTask.Add(scen.Energy);
                        }
                        contender.Benchmark.EnergyPerTask.Remove(subcat.Scenarios.Min(s => s.Energy));
                    }
                }
                contender.Benchmark.TotalEnergy = contender.Benchmark.EnergyPerTask.Sum();
            }




            //headers (on left side)
            var headerstacky = new StackPanel()
            {
                Name = "benchboard_headers",
                Width = 150,
                HorizontalAlignment = HorizontalAlignment.Left,
                Background = getColorFromHex("#eeeeee")
            };
            //placement
            headerstacky.Children.Add(new TextBlock()
            {
                Text = "Placement",
            });
            //player names
            headerstacky.Children.Add(new TextBlock()
            {
                Text = "Name",
            });
            //scenario names
            for (int i = 0; i < bench.Categories.Length; i++)
                for (int j = 0; j < bench.Categories[i].Subcategories.Length; j++)
                    for (int k = 0; k < bench.Categories[i].Subcategories[j].Scenarios.Length; k++)
                    {
                        headerstacky.Children.Add(new TextBlock()
                        {
                            Name = $"benchboardheader_{i}_{j}_{k}",
                            Text = bench.Categories[i].Subcategories[j].Scenarios[k].Name,
                        });
                    }

            headerstacky.Children.Add(new TextBlock()
            {
                Name = $"header_energytotal",
                Text = "Total Energy"
            });
            //benchStacky2.Children.Add(headerstacky);


            BenchmarkLeaderboardContenders = BenchmarkLeaderboardContenders.OrderByDescending(c => c.Benchmark.TotalEnergy).ToList();

            int placement = 0;
            foreach (var contender in BenchmarkLeaderboardContenders)
            {
                placement++;
                var playerstacky = new StackPanel()
                {
                    Width = 80,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Background = getColorFromHex("#eeeeee")
                };
                //placement
                playerstacky.Children.Add(new TextBlock()
                {
                    Text = $"#{placement}",
                    TextAlignment = TextAlignment.Center,
                });
                //player names
                playerstacky.Children.Add(new TextBlock()
                {
                    Text = contender.UserName,
                    TextAlignment = TextAlignment.Center,
                });
                //scenario names
                for (int i = 0; i < bench.Categories.Length; i++)
                    for (int j = 0; j < bench.Categories[i].Subcategories.Length; j++)
                        for (int k = 0; k < bench.Categories[i].Subcategories[j].Scenarios.Length; k++)
                        {
                            playerstacky.Children.Add(new TextBlock()
                            {
                                Name = $"benchcontenderresult_{i}_{j}_{k}_{placement}",
                                Text = contender.Benchmark.Categories[i].Subcategories[j].Scenarios[k].Score.ToString(),
                                TextAlignment = TextAlignment.Center,
                            });
                        }


                playerstacky.Children.Add(new TextBlock()
                {
                    Name = $"header_total",
                    Text = contender.Benchmark.TotalEnergy.ToString(),
                    TextAlignment = TextAlignment.Center,
                });
                // benchLeaderboardDocky.Children.Add(playerstacky);
            }
        }
    }
}
