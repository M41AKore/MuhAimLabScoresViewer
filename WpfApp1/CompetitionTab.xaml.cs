using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using static MuhAimLabScoresViewer.APIStuff;
using static MuhAimLabScoresViewer.APIStuff.LeaderboardResult;
using static MuhAimLabScoresViewer.Helper;
using static MuhAimLabScoresViewer.ObjectsAndStructs;

namespace MuhAimLabScoresViewer
{
    /// <summary>
    /// Interaction logic for CompetitionTab.xaml
    /// </summary>
    public partial class CompetitionTab : Page
    {
        public static ViewModel viewModel;
        Stopwatch timer = new Stopwatch();
        public static Competition currentComp;
        public static List<KeyValuePair<string, TextBlock>> compScoreFieldLookup;
        public DateTime nextEndingPart_DateTime_forTimer;
        Task countdownTimerTask = null;

        public CompetitionTab()
        {
            InitializeComponent();
            viewModel = MainWindow.viewModel;
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            CompetitionStacky.Children.Clear();
            CompInfoDocky.Children.Clear();
            myDocky.Children.Clear();
            if (viewModel.LastCompetitionPath != null) HandleFile(viewModel.LastCompetitionPath);
        }
        public void NameTB_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && e.ClickCount == 2)
            {
                var tb = sender as TextBlock;
                var part = tb.Name.Substring(11, tb.Name.Length - 11);
                string link = generateLink(part); // $"playtask_{parts[0]}_{parts[1]}",   0 = authorid, 1 = workshopid

                ProcessStartInfo processStartInfo = new ProcessStartInfo(link);
                processStartInfo.UseShellExecute = true;
                System.Diagnostics.Process.Start(processStartInfo);
            }
        }

        private void DragDropInput(object sender, System.Windows.DragEventArgs e) => getFileDrop(e);
        private void getFileDrop(System.Windows.DragEventArgs e)
        {
            if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop))
                foreach (string file in (string[])e.Data.GetData(System.Windows.DataFormats.FileDrop))
                    HandleFile(file);
        }
        public void HandleFile(string filepath)
        {
            timer.Start();
            string filename = filepath.Split('\\').Last();

            if (!string.IsNullOrEmpty(viewModel.SteamLibraryPath) && Directory.Exists(viewModel.SteamLibraryPath))
            {
                if (filename.ToLower().Contains("competition") || filename.ToLower().Contains("comp"))
                {
                    try
                    {
                        var newcomp = XmlSerializer.deserializeXml<Competition>(filepath);
                        if (newcomp != null)
                        {
                            currentComp = newcomp;
                            viewModel.LastCompetitionPath = filepath;
                            loadCompetitionToGUI();
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.log("Error during competition loading!" + Environment.NewLine + e.Message.ToString());
                        currentComp = null;
                        viewModel.LastCompetitionPath = null;
                        MainWindow.Instance.showMessageBox("Could not load this Competition file!");
                    }

                }
            }
            else MainWindow.Instance.showMessageBox("please enter 'SteamLibraryPath' in Settings!");
        }

        private async Task loadCompetitionToGUI()
        {
            var tasks = new List<Task>();
            compScoreFieldLookup = new List<KeyValuePair<string, TextBlock>>();
            CompetitionStacky.Children.Clear();

            for (int i = 0; i < currentComp.Parts.Length; i++)
            {
                var stacky = new StackPanel();
                for (int j = 0; j < currentComp.Parts[i].Scenarios.Length; j++)
                {
                    var stuff = Helper.getLevelAndWeaponForTask(currentComp.Parts[i].Scenarios[j].TaskName);
                    if (stuff != null)
                        tasks.Add(Task.Run(() => APIStuff.runLeaderboardQuery(stuff.level.Replace("%20", " "), stuff.weapon, 0, 30) //only 20 are relevant
                            .ContinueWith(result => addCompLeaderboard(result.Result, stuff.taskname))));

                    var docky = new DockPanel();
                    var parts = getAuthorIdAndWorkshopIdFromTaskName(currentComp.Parts[i].Scenarios[j].TaskName)?.Split(' '); //deeplink
                    var nameTB = new TextBlock()
                    {
                        Name = parts != null && parts.Length >= 2 ? $"playtask_{parts[0]}_{parts[1]}" : $"playtask_{i}_{j}",
                        Text = currentComp.Parts[i].Scenarios[j].DisplayName,
                        Width = 150, //220
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Background = Brushes.LightGray
                    };
                    nameTB.MouseDown += NameTB_MouseDown;
                    docky.Children.Add(nameTB);

                    var scoreTB = new TextBlock()
                    {
                        Name = $"score_{i}_{j}",
                        Width = 100,
                        TextAlignment = TextAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Background = Brushes.White
                    };
                    compScoreFieldLookup.Add(new KeyValuePair<string, TextBlock>(scoreTB.Name, scoreTB));
                    docky.Children.Add(scoreTB);

                    stacky.Children.Add(docky);
                }
                CompetitionStacky.Children.Add(new Border()
                {
                    Background = getColorFromHex("#eeeeee"),
                    BorderBrush = System.Windows.Media.Brushes.Black,
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(5),
                    Padding = new Thickness(2),
                    Child = stacky
                });
            }

            if (!string.IsNullOrEmpty(viewModel.klutchId))
            {
                await Task.WhenAll(tasks);
                Competition.buildCompContenders();
                buildCompLeaderboardToGUI();
            }
            else MainWindow.Instance.showMessageBox("please set 'klutchId' in Settings!");
        }
        private void updateCompetitionUIWithHighscore(string taskName, leaderboardData user)
        {
            Trace.WriteLine("updating '" + taskName + "'...");
            if (user == null) return;

            string textblockID = "";
            for (int i = 0; i < currentComp.Parts.Length; i++)
                for (int j = 0; j < currentComp.Parts[i].Scenarios.Length; j++)
                    if (currentComp.Parts[i].Scenarios[j].TaskName == taskName)
                    {
                        textblockID = $"score_{i}_{j}";
                        this.Dispatcher.Invoke(() =>
                        {
                            var field = compScoreFieldLookup.FirstOrDefault(f => f.Key == textblockID).Value;
                            if (field != null) field.Text = user.score.ToString();
                        });
                        return;
                    }
        }
        public TextBlock findPersonalCompetitionStatsScoreField(string fieldname)
        {
            TextBlock result = null;

            foreach (var c1 in CompetitionStacky.Children)
                if (c1 is Border)
                    foreach (DockPanel docky in ((c1 as Border).Child as StackPanel).Children)
                        foreach (var field in docky.Children)
                        {
                            if (field is TextBlock)
                            {
                                var tb = field as TextBlock;
                                if (tb != null && tb.Name == fieldname) return tb;
                            }
                        }

            return result;
        }

        private void addCompLeaderboard(LeaderboardResult result, string taskName)
        {
            Trace.WriteLine(taskName);

            for (int i = 0; i < currentComp.Parts.Length; i++)
                for (int j = 0; j < currentComp.Parts[i].Scenarios.Length; j++)
                    if (currentComp.Parts[i].Scenarios[j].TaskName == taskName)
                    {
                        currentComp.Parts[i].Scenarios[j].leaderboard = new CompetitionTaskLeaderboard()
                        {
                            TaskName = taskName,
                            Leaderboard = result.aimlab.leaderboard
                        };
                        Trace.WriteLine("added leaderboard for " + taskName);
                        updateCompetitionUIWithHighscore(taskName, result.aimlab.leaderboard.data.FirstOrDefault(r => r.user_id == viewModel.klutchId));
                    }
        }
        private void buildCompLeaderboardToGUI()
        {
            Competition_Title.Text = currentComp.Title;
            Competition_Title.FontWeight = FontWeights.Bold;
            CompInfoDocky.Children.Clear();
            myDocky.Children.Clear();
            addCompetitionTimerGUI(CompInfoDocky); //timer
            addCompetitionLeaderboardGUIHeaders(CompInfoDocky); // headers
            addCompetitionLeaderboardGUIPlayerData(myDocky); // playerdata
        }
        private void addCompetitionLeaderboardGUIPlayerData(DockPanel boardDocky)
        {
            for (int i = 0; i < currentComp.competitionContenders.Count; i++)
            {
                var playerStacky = new StackPanel()
                {
                    Width = 80,
                    Background = getColorFromHex("#ffffff")
                };

                //placement
                playerStacky.Children.Add(new TextBlock()
                {
                    Name = $"placement_{i}",
                    Text = $"#{i + 1}",
                    Width = 80,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    TextAlignment = TextAlignment.Center,
                });

                //name
                playerStacky.Children.Add(new TextBlock()
                {
                    Name = $"name_{i}",
                    Text = currentComp.competitionContenders[i].Name,
                    Width = 80,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    TextAlignment = TextAlignment.Center,
                });

                //scores
                for (int j = 0; j < currentComp.competitionContenders[i].partResults.Length; j++)
                    for (int y = 0; y < currentComp.competitionContenders[i].partResults[j].taskResults.Length; y++)
                        playerStacky.Children.Add(new TextBlock()
                        {
                            Name = $"score_{i}_{j}_{y}",
                            Text = currentComp.competitionContenders[i].partResults[j].taskResults[y].score.ToString(),
                            ToolTip = currentComp.competitionContenders[i].partResults[j].taskResults[y].points.ToString("0.##"),
                            Width = 40,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            TextAlignment = TextAlignment.Center,
                        });

                //points per part
                for (int j = 0; j < currentComp.competitionContenders[i].partResults.Length; j++)
                    playerStacky.Children.Add(new TextBlock()
                    {
                        Name = $"partscore_{i}_{j}",
                        Text = Math.Round(currentComp.competitionContenders[i].partResults[j].partPoints, 2, MidpointRounding.AwayFromZero).ToString(),
                        Width = 40,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        TextAlignment = TextAlignment.Center,
                    });

                //total points
                playerStacky.Children.Add(new TextBlock()
                {
                    Name = $"totalscore_{i}",
                    Text = Math.Round(currentComp.competitionContenders[i].totalPoints, 2, MidpointRounding.AwayFromZero).ToString(),
                    Width = 40,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    TextAlignment = TextAlignment.Center,
                });

                boardDocky.Children.Add(new Border()
                {
                    Background = getColorFromHex("#eeeeee"),
                    BorderBrush = Brushes.Black,
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(3),
                    Padding = new Thickness(2),
                    Child = playerStacky
                });
            }
        }
        private void addCompetitionLeaderboardGUIHeaders(DockPanel boardDocky)
        {
            //headers (on left side)
            var headerstacky = new StackPanel()
            {
                Name = "competitionboard_headers",
                Width = 150,
                HorizontalAlignment = HorizontalAlignment.Left,
                Background = getColorFromHex("#eeeeee")
            };
            //placement
            headerstacky.Children.Add(new TextBlock()
            {
                Name = "header_placement",
                Text = "Placement",
            });
            //player names
            headerstacky.Children.Add(new TextBlock()
            {
                Name = "header_name",
                Text = "Name",
            });
            //scenario names
            for (int i = 0; i < currentComp.Parts.Length; i++)
            {
                for (int j = 0; j < currentComp.Parts[i].Scenarios.Length; j++)
                {
                    headerstacky.Children.Add(new TextBlock()
                    {
                        Name = $"header_{i}_{j}",
                        Text = currentComp.Parts[i].Scenarios[j].DisplayName,
                    });
                }
            }
            //part names
            for (int i = 0; i < currentComp.Parts.Length; i++)
            {
                headerstacky.Children.Add(new TextBlock()
                {
                    Name = $"header_part_{i}",
                    Text = $"Part {i + 1} points",
                });
            }
            headerstacky.Children.Add(new TextBlock()
            {
                Name = $"header_total",
                Text = "Total points"
            });

            boardDocky.Children.Add(headerstacky);
        }

        private void addCompetitionTimerGUI(DockPanel boardDocky)
        {
            string? s = null;
            PartExpiraton nextEndingPart = null;

            for (int i = 0; i < currentComp.Parts.Length; i++)
                if (DateTime.Parse(currentComp.Parts[i].Startdate) < DateTime.UtcNow && DateTime.Parse(currentComp.Parts[i].Enddate) > DateTime.UtcNow)
                {
                    //active
                    if (s == null) s = (i + 1).ToString();
                    else s = $",{i + 1}";

                    var timeTillExpiration = DateTime.Parse(currentComp.Parts[i].Enddate) - DateTime.UtcNow;
                    if (nextEndingPart == null || timeTillExpiration < DateTime.Parse(nextEndingPart.part.Enddate) - DateTime.UtcNow)
                        nextEndingPart = new PartExpiraton()
                        {
                            part = currentComp.Parts[i],
                            timeTillExpiration = timeTillExpiration
                        };
                }

            if (s == null && nextEndingPart == null)
            {
                lbl_activepart.Text = "None";
                lbl_endson.Text = "Ended on:";
                var end = currentComp.Parts.LastOrDefault()?.Enddate;
                lbl_partendtimer.Text = end != null ? $"{end} (UTC)" : "Unknown";
                if (countdownTimerTask != null && countdownTimerTask.Status == TaskStatus.Running) countdownTimerTask.Dispose();
            }
            else
            {
                lbl_activepart.Text = s;
                if (nextEndingPart.timeTillExpiration > TimeSpan.FromHours(48))
                {
                    lbl_endson.Text = "Ends on:";
                    lbl_partendtimer.Foreground = Brushes.Black;
                    lbl_partendtimer.Text = $"{nextEndingPart.part.Enddate} (UTC)";
                }
                else
                {
                    lbl_endson.Text = "Ends in:";
                    int hours = nextEndingPart.timeTillExpiration.Days > 0 ? nextEndingPart.timeTillExpiration.Hours + 24 : nextEndingPart.timeTillExpiration.Hours;
                    lbl_partendtimer.Text = $"{hours}:{nextEndingPart.timeTillExpiration.Minutes}:{nextEndingPart.timeTillExpiration.Seconds}";

                    //timer
                    nextEndingPart_DateTime_forTimer = DateTime.Parse(nextEndingPart.part.Enddate);
                    countdownTimerTask = Task.Run(() =>
                    {
                        while (nextEndingPart_DateTime_forTimer > DateTime.UtcNow)
                        {
                            updateCountdownTimer();
                            Thread.Sleep(1000);
                        }

                        //time over, set final output
                        this.Dispatcher.Invoke(() =>
                        {
                            if (nextEndingPart.part == currentComp.Parts.LastOrDefault()) //last part, comp over
                            {
                                lbl_activepart.Text = "None";
                                lbl_endson.Text = "Ended on:";
                                var end = nextEndingPart.part?.Enddate;
                                lbl_partendtimer.Foreground = Brushes.Black;
                                lbl_partendtimer.Text = end != null ? $"{end} (UTC)" : "Unknown";
                            }
                            else addCompetitionTimerGUI(boardDocky); //not last part, go to next 
                        });
                    });
                }
            }
        }
        private void updateCountdownTimer()
        {
            var timeLeft = nextEndingPart_DateTime_forTimer - DateTime.UtcNow;
            if (timeLeft > TimeSpan.Zero)
            {
                int hoursTotal = timeLeft.Days > 0 ? timeLeft.Hours + 24 : timeLeft.Hours;
                string hours = hoursTotal < 10 ? $"0{hoursTotal}" : hoursTotal.ToString();
                string minutes = timeLeft.Minutes < 10 ? $"0{timeLeft.Minutes}" : timeLeft.Minutes.ToString();
                string seconds = timeLeft.Seconds < 10 ? $"0{timeLeft.Seconds}" : timeLeft.Seconds.ToString();
                this.Dispatcher.Invoke(() =>
                {
                    lbl_partendtimer.Text = $"{hours}:{minutes}:{seconds}";
                    if (hoursTotal < 1) lbl_partendtimer.Foreground = Brushes.Red;
                });
            }
        }
    }
}
