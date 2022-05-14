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
using static OBS.libobs;
using System.Data.SQLite;

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

        public static string settingsPath = "./settings.xml";
        public static Settings currentSettings;
        public static Competition currentComp;
        public static Benchmark currentBenchmark;

        Stopwatch timer = new Stopwatch();

        public DateTime nextEndingPart_DateTime_forTimer;
        Task countdownTimerTask = null;

        ScreenCaptureNvenc recorder = null;
        string outputFileName;
        string outputPath;
        string h264 = "";
        string mp4 = "";

        KeyboardHook hook = new KeyboardHook();
        private bool registeredHotkey = false;

        public static List<KeyValuePair<string, TextBlock>> compScoreFieldLookup;

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

            Logger.setup();
            string LocalLowPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).Replace("Roaming", "LocalLow");
            LiveTracker.LocalDBFile = Path.Combine(LocalLowPath, "Statespace\\aimlab_tb\\klutch.bytes");

            Settings.loadSettings();

            if (viewModel.LiveTrackerEnabled) LiveTracker.createLiveTrackerGUI();
        }

        private void TaskButton_Click(object sender, RoutedEventArgs e)
        {
            TasksTab.Visibility = Visibility.Visible;
            TaskButton_BottomBorder.Visibility = Visibility.Hidden;

            BenchmarksTab.Visibility = Visibility.Collapsed;
            BenchmarkButton_BottomBorder.Visibility = Visibility.Visible;

            CompetitionsTab.Visibility = Visibility.Collapsed;
            CompetitionButton_BottomBorder.Visibility = Visibility.Visible;

            AimLabHistoryViewerTab.Visibility = Visibility.Collapsed;
            AimLabHistoryButton_BottomBorder.Visibility = Visibility.Visible;

            LivetrackerTab.Visibility = Visibility.Collapsed;
            LiveTrackerButton_BottomBorder.Visibility = Visibility.Visible;

            SettingsTab.Visibility = Visibility.Collapsed;
            SettingsButton_BottomBorder.Visibility = Visibility.Visible;

            this.Height = 500;
            this.Width = 700;
        }
        private void BenchmarkButton_Click(object sender, RoutedEventArgs e)
        {
            TasksTab.Visibility = Visibility.Collapsed;
            TaskButton_BottomBorder.Visibility = Visibility.Visible;

            BenchmarksTab.Visibility = Visibility.Visible;
            BenchmarkButton_BottomBorder.Visibility = Visibility.Hidden;

            CompetitionsTab.Visibility = Visibility.Collapsed;
            CompetitionButton_BottomBorder.Visibility = Visibility.Visible;

            AimLabHistoryViewerTab.Visibility = Visibility.Collapsed;
            AimLabHistoryButton_BottomBorder.Visibility = Visibility.Visible;

            LivetrackerTab.Visibility = Visibility.Collapsed;
            LiveTrackerButton_BottomBorder.Visibility = Visibility.Visible;

            SettingsTab.Visibility = Visibility.Collapsed;
            SettingsButton_BottomBorder.Visibility = Visibility.Visible;

            this.Height = 840;
            this.Width = 800;
        }
        private void CompetitionButton_Click(object sender, RoutedEventArgs e)
        {
            TasksTab.Visibility = Visibility.Collapsed;
            TaskButton_BottomBorder.Visibility = Visibility.Visible;

            BenchmarksTab.Visibility = Visibility.Collapsed;
            BenchmarkButton_BottomBorder.Visibility = Visibility.Visible;

            CompetitionsTab.Visibility = Visibility.Visible;
            CompetitionButton_BottomBorder.Visibility = Visibility.Hidden;

            AimLabHistoryViewerTab.Visibility = Visibility.Collapsed;
            AimLabHistoryButton_BottomBorder.Visibility = Visibility.Visible;

            LivetrackerTab.Visibility = Visibility.Collapsed;
            LiveTrackerButton_BottomBorder.Visibility = Visibility.Visible;

            SettingsTab.Visibility = Visibility.Collapsed;
            SettingsButton_BottomBorder.Visibility = Visibility.Visible;

            this.Height = 580;
            this.Width = 1060;
        }
        private void AimLabHistoryButton_Click(object sender, RoutedEventArgs e)
        {
            AimLabHistoryViewerTab.Visibility = Visibility.Visible;
            AimLabHistoryButton_BottomBorder.Visibility = Visibility.Hidden;

            TasksTab.Visibility = Visibility.Collapsed;
            TaskButton_BottomBorder.Visibility = Visibility.Visible;

            BenchmarksTab.Visibility = Visibility.Collapsed;
            BenchmarkButton_BottomBorder.Visibility = Visibility.Visible;

            CompetitionsTab.Visibility = Visibility.Collapsed;
            CompetitionButton_BottomBorder.Visibility = Visibility.Visible;

            LivetrackerTab.Visibility = Visibility.Collapsed;
            LiveTrackerButton_BottomBorder.Visibility = Visibility.Visible;

            SettingsTab.Visibility = Visibility.Collapsed;
            SettingsButton_BottomBorder.Visibility = Visibility.Visible;

            this.Height = 750;
            this.Width = 1050;
        }
        private void LiveTrackerButton_Click(object sender, RoutedEventArgs e)
        {
            LivetrackerTab.Visibility = Visibility.Visible;
            LiveTrackerButton_BottomBorder.Visibility = Visibility.Hidden;

            TasksTab.Visibility = Visibility.Collapsed;
            TaskButton_BottomBorder.Visibility = Visibility.Visible;

            BenchmarksTab.Visibility = Visibility.Collapsed;
            BenchmarkButton_BottomBorder.Visibility = Visibility.Visible;

            CompetitionsTab.Visibility = Visibility.Collapsed;
            CompetitionButton_BottomBorder.Visibility = Visibility.Visible;

            AimLabHistoryViewerTab.Visibility = Visibility.Collapsed;
            AimLabHistoryButton_BottomBorder.Visibility = Visibility.Visible;

            SettingsTab.Visibility = Visibility.Collapsed;
            SettingsButton_BottomBorder.Visibility = Visibility.Visible;

            this.Height = 750;
            this.Width = 800;
        }
        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            TasksTab.Visibility = Visibility.Collapsed;
            TaskButton_BottomBorder.Visibility = Visibility.Visible;

            BenchmarksTab.Visibility = Visibility.Collapsed;
            BenchmarkButton_BottomBorder.Visibility = Visibility.Visible;

            CompetitionsTab.Visibility = Visibility.Collapsed;
            CompetitionButton_BottomBorder.Visibility = Visibility.Visible;

            AimLabHistoryViewerTab.Visibility = Visibility.Collapsed;
            AimLabHistoryButton_BottomBorder.Visibility = Visibility.Visible;

            LivetrackerTab.Visibility = Visibility.Collapsed;
            LiveTrackerButton_BottomBorder.Visibility = Visibility.Visible;

            SettingsTab.Visibility = Visibility.Visible;
            SettingsButton_BottomBorder.Visibility = Visibility.Hidden;

            this.Height = 600;
            this.Width = 700;
        }
        private void TextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter) getLeaderboardFor((sender as System.Windows.Controls.TextBox).Text);
        }
        private void DragDropInput(object sender, System.Windows.DragEventArgs e) => getFileDrop(e);
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var part = btn.Name.Substring(11, btn.Name.Length - 11);
            string link = generateLink(part); // $"playbutton_{parts[0]}_{parts[1]}",   0 = authorid, 1 = workshopid

            ProcessStartInfo processStartInfo = new ProcessStartInfo(link);
            processStartInfo.UseShellExecute = true;
            System.Diagnostics.Process.Start(processStartInfo);
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
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            benchStacky.Children.Clear();
            if (viewModel.LastBenchmarkPath != null) HandleFile(viewModel.LastBenchmarkPath);
        }
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            CompetitionStacky.Children.Clear();
            CompInfoDocky.Children.Clear();
            myDocky.Children.Clear();
            if (viewModel.LastCompetitionPath != null) HandleFile(viewModel.LastCompetitionPath);
        }
        private void klutchIdFinder_Scenario_GotFocus(object sender, RoutedEventArgs e)
        {
            var tb = sender as System.Windows.Controls.TextBox;
            if (tb != null && tb.Text == "Enter scenario") tb.Text = "";
        }
        private void klutchIdFinder_Username_GotFocus(object sender, RoutedEventArgs e)
        {
            var tb = sender as System.Windows.Controls.TextBox;
            if (tb != null && tb.Text == "Enter username") tb.Text = "";
        }
        private void klutchIdFinder_Scenario_LostFocus(object sender, RoutedEventArgs e)
        {
            var tb = sender as System.Windows.Controls.TextBox;
            if (tb != null && string.IsNullOrEmpty(tb.Text)) tb.Text = "Enter scenario";
        }
        private void klutchIdFinder_Username_LostFocus(object sender, RoutedEventArgs e)
        {
            var tb = sender as System.Windows.Controls.TextBox;
            if (tb != null && string.IsNullOrEmpty(tb.Text)) tb.Text = "Enter username";
        }
        private void Button_Click_5(object sender, RoutedEventArgs e) => buildklutchIdCall();
        private void RecordingButton_Click(object sender, RoutedEventArgs e)
        {
            if (viewModel.IsRecording) stopRecording();
            else startRecording();
        }
        private void recordHotkeySet_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn != null)
            {
                btn.Content = "press key";
                btn.KeyUp += Btn_KeyDown;
            }
        }
        private void Btn_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            var btn = sender as Button;
            if (btn != null)
            {
                btn.KeyUp -= Btn_KeyDown;
                btn.Content = e.Key.ToString();
            }

            if (currentSettings.RecordingHotKey != null) hook.UnregisterHotkeys(); //gets rid of previous hotkey
            currentSettings.RecordingHotKey = (Keys)KeyInterop.VirtualKeyFromKey(e.Key);
            registerRecordingHotkey(currentSettings);
        }
        private void getFileDrop(System.Windows.DragEventArgs e)
        {
            if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop))
                foreach (string file in (string[])e.Data.GetData(System.Windows.DataFormats.FileDrop))
                    HandleFile(file);
        }
        private void hook_KeyPressed(object sender, KeyPressedEventArgs e)
        {
            if (currentSettings.alsoTakeScreenshot) takeScreenshot();
            saveReplay();
        }
        private void Button_Click_3(object sender, RoutedEventArgs e) => LiveTracker.simulateKeyPress(currentSettings.OBS_Hotkey);
        private void Button_Click_4(object sender, RoutedEventArgs e) => AimLabHistoryViewer.pullDataFromLocalDB();
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
                msgBox.output.Text = text;

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
                this.Dispatcher.Invoke(() =>
                {
                    autoRecordStatus_Output.Text = s;
                });
                Thread.Sleep(3000);
                this.Dispatcher.Invoke(() =>
                {
                    autoRecordStatus_Output.Text = "Listening...";
                });
            });
        }

        public void registerRecordingHotkey(Settings settings)
        {
            // register the event that is fired after the key press.
            if (!registeredHotkey) hook.KeyPressed += new EventHandler<KeyPressedEventArgs>(hook_KeyPressed);
            // register the control + alt + F12 combination as hot key.
            hook.RegisterHotKey(ModifierKeys.None, (Keys)settings.RecordingHotKey); //ModifierKeys.Control | ModifierKeys.Alt, Keys.F12
            registeredHotkey = true;
        }
        public void HandleFile(string filepath)
        {
            timer.Start();
            string filename = filepath.Split('\\').Last();

            if (!string.IsNullOrEmpty(viewModel.SteamLibraryPath) && Directory.Exists(viewModel.SteamLibraryPath))
            {
                if (filename.ToLower().Contains("benchmark") || filename.ToLower().Contains("bench"))
                {
                    try
                    {
                        var newbench = XmlSerializer.deserializeXml<Benchmark>(filepath);
                        if (newbench != null)
                        {
                            currentBenchmark = newbench;
                            viewModel.LastBenchmarkPath = filepath;
                            Benchmark.addBenchmarkGUIHeaders(benchStacky);
                            Benchmark.addBenchmarkScores(benchStacky);
                            loadBenchmarkToGUI(newbench);
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.log("Error during benchmark loading!" + Environment.NewLine + e.Message.ToString());
                        currentBenchmark = null;
                        viewModel.LastBenchmarkPath = null;
                        showMessageBox("Could not load this Benchmark file!");
                    }
                }
                else if (filename.ToLower().Contains("competition") || filename.ToLower().Contains("comp"))
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
                        showMessageBox("Could not load this Competition file!");
                    }

                }
                else if (filename.ToLower().Contains("taskData") || filename.ToLower().Contains(".json"))
                {
                    try
                    {
                        if (File.Exists(filepath))
                        {
                            var item = AimLabHistoryViewer.getData(filepath);
                            if (item != null) AimLabHistoryViewer.sortTasks(item.historyEntries);
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.log("Error during taskdata loading!" + Environment.NewLine + e.Message.ToString());
                        showMessageBox("Could not load this TaskData file!");
                    }
                }
            }
            else showMessageBox("please enter 'SteamLibraryPath' in Settings!");
        }

        //merge these into one pls
        private async void launchBenchmarkUpdates(List<HighscoreUpdateCall> calllist, Action<Task<HighscoreUpdateCall>> receiver)
        {
            timer.Restart();

            List<Task> tasks = new List<Task>();
            foreach (var call in calllist)
                tasks.Add(Task.Run(async () => await APIStuff.getHighscore(call).ContinueWith(result => receiver(result)))); // updateBenchmarkWithHighscore(result));

            await Task.WhenAll(tasks.ToArray());
            string rankName = Benchmark.calculateBenchmarkRank(benchStacky);
            Txt_BenchmarkRank.Text = rankName;
            //Txt_BenchmarkRank.Foreground = getColorFromHex(currentBenchmark.Ranks.FirstOrDefault(r => r.Name == rankName).Color);
            Txt_BenchmarkEnergy.Text = ((int)currentBenchmark.TotalEnergy).ToString();
        }
        private async void launchCompetitionUpdates(List<HighscoreUpdateCall> calllist, Action<Task<HighscoreUpdateCall>> receiver)
        {
            timer.Restart();
            List<Task> tasks = new List<Task>();
            foreach (var call in calllist)
                tasks.Add(Task.Run(async () => await APIStuff.getHighscore(call).ContinueWith(result => receiver(result))));

            await Task.WhenAll(tasks.ToArray());
            //
        }

        //task leaderboard
        private void getLeaderboardFor(string taskname)
        {
            if (!Directory.Exists(viewModel.SteamLibraryPath))
            {
                showMessageBox("please set SteamLibraryPath in Settings!");
                return;
            }

            string call = buildAPICallFromTaskName(taskname);
            if (call == null)
            {
                var t = Task.Run(() =>
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        leaderboardStacky.Children.Clear();
                        searchInfo.Visibility = Visibility.Visible;
                    });
                    Thread.Sleep(3000);
                    this.Dispatcher.Invoke(() =>
                    {
                        searchInfo.Visibility = Visibility.Hidden;
                    });
                });
            }
            else
            {
                APIStuff.httpstuff(call).ContinueWith(item => populateleaderboard(item.Result.results));
            }
        }
        private void populateleaderboard(Result[] results)
        {
            try
            {
                this.Dispatcher.Invoke(() =>
                {
                    addTaskLeaderboardHeaders(leaderboardheaderStacky);
                    addTaskLeaderboardData(results, leaderboardStacky);
                });
            }
            catch (Exception e)
            {
                showMessageBox(e.Message);
            }
        }
        private void addTaskLeaderboardHeaders(StackPanel parent)
        {
            var headerDocky = new DockPanel()
            {
                Margin = new Thickness(-15, 2, 5, 2),
                //Background = Brushes.Red
                Width = 380,
            };

            headerDocky.Children.Add(new TextBlock()
            {
                Text = "Place",
                Width = 40,
            });
            headerDocky.Children.Add(new TextBlock()
            {
                Text = "Player",
                Width = 120,
            });
            headerDocky.Children.Add(new TextBlock()
            {
                Text = "Score",
                Width = 60,
            });
            headerDocky.Children.Add(new TextBlock()
            {
                Text = "Hits",
                Width = 40,
            });
            headerDocky.Children.Add(new TextBlock()
            {
                Text = "Misses",
                Width = 40,
            });
            headerDocky.Children.Add(new TextBlock()
            {
                Text = "Accuracy",
                Width = 60,
            });

            parent.Children.Add(headerDocky);
        }
        private void addTaskLeaderboardData(Result[] results, StackPanel parent)
        {
            leaderboardStacky.Children.Clear();

            for (int i = 0; i < results.Length; i++)
            {
                var entryDocky = new DockPanel()
                {
                    Margin = new Thickness(5, 2, 5, 2),
                    //Background = Brushes.Red
                    Width = 380
                };

                //placement
                entryDocky.Children.Add(new TextBlock()
                {
                    Width = 40,
                    Text = $"#{i + 1}",
                });

                //player name
                entryDocky.Children.Add(new TextBlock()
                {
                    Width = 120,
                    Text = results[i].username,
                });

                //score
                entryDocky.Children.Add(new TextBlock()
                {
                    Width = 60,
                    Text = results[i].score.ToString(),
                });

                //hits
                entryDocky.Children.Add(new TextBlock()
                {
                    Width = 40,
                    Text = results[i].hitstotal.ToString(),
                });

                //misses
                entryDocky.Children.Add(new TextBlock()
                {
                    Width = 40,
                    Text = results[i].missestotal.ToString(),
                });

                //accuracy
                entryDocky.Children.Add(new TextBlock()
                {
                    Width = 60,
                    Text = results[i].acctotal,
                });

                leaderboardStacky.Children.Add(entryDocky);
            }
        }

        //benchmark scores
        private void loadBenchmarkToGUI(Benchmark bench)
        {
            Trace.WriteLine("time taken for reading file = " + timer.ElapsedMilliseconds);
            timer.Restart();
            var calllist = new List<HighscoreUpdateCall>();

            foreach (var category in bench.Categories)
            {
                foreach (var subcategory in category.Subcategories)
                {
                    foreach (var scenario in subcategory.Scenarios)
                    {
                        var updateCall = new HighscoreUpdateCall()
                        {
                            apicall = buildAPICallFromTaskName(scenario.Name),
                            taskname = scenario.Name,
                            highscore = "0"
                        };
                        if (updateCall.apicall != null) calllist.Add(updateCall);

                        if (!string.IsNullOrEmpty(scenario.AlternativeName))
                        {
                            var c = buildAPICallFromTaskName(scenario.AlternativeName);
                            var updateCall2 = new HighscoreUpdateCall()
                            {
                                apicall = c,
                                taskname = scenario.AlternativeName,
                                parentTaskName = scenario.Name,
                                highscore = "0"
                            };
                            if (updateCall2.apicall != null) calllist.Add(updateCall2);
                        }
                    }
                }
            }

            Trace.WriteLine("time for building datagrids and calls: " + timer.ElapsedMilliseconds);

            if (!string.IsNullOrEmpty(viewModel.klutchId)) launchBenchmarkUpdates(calllist, updateBenchmarkWithHighscore);
            else showMessageBox("please set 'klutchId' in Settings!");
        }
        private string calculateUIScoreItemName(Task<HighscoreUpdateCall> call)
        {
            for (int i = 0; i < currentBenchmark.Categories.Length; i++)
                for (int j = 0; j < currentBenchmark.Categories[i].Subcategories.Length; j++)
                    for (int k = 0; k < currentBenchmark.Categories[i].Subcategories[j].Scenarios.Length; k++)
                    {
                        if (call.Result.parentTaskName != null)
                        {
                            if (currentBenchmark.Categories[i].Subcategories[j].Scenarios[k].Name == call.Result.parentTaskName) return $"score_{i}_{j}_{k}";
                        }
                        else
                        {
                            if (currentBenchmark.Categories[i].Subcategories[j].Scenarios[k].Name == call.Result.taskname) return $"score_{i}_{j}_{k}";
                        }
                    }

            return null;
        }
        private void updateBenchmarkWithHighscore(Task<HighscoreUpdateCall> call)
        {
            timer.Restart();
            Trace.WriteLine("updating '" + call.Result.taskname + "'...");
            var targetName = calculateUIScoreItemName(call);
            if (targetName != null)
            {
                this.Dispatcher.Invoke(() =>
                {
                    //TextBlock tb = Benchmark.findBenchmarkScoreFieldWithName(targetName, benchStacky); //create a lookup table so this doesn't have to be found by UI thread
                    TextBlock tb = Benchmark.benchScoreFieldLookup.FirstOrDefault(f => f.Key == targetName).Value; // ^did
                    if (tb != null && int.TryParse(tb.Text, out int parentTaskScore) && parentTaskScore < int.Parse(call.Result.highscore))
                        tb.Text = call.Result.highscore;
                });
            }
            Trace.WriteLine($"updated '{call.Result.taskname}' in {timer.ElapsedMilliseconds}ms!");
        }

        //personal competition score display
        private void loadCompetitionToGUI()
        {
            var calllist = new List<HighscoreUpdateCall>();
            compScoreFieldLookup = new List<KeyValuePair<string, TextBlock>>();
            CompetitionStacky.Children.Clear();

            for (int i = 0; i < currentComp.Parts.Length; i++)
            {
                var stacky = new StackPanel();
                for (int j = 0; j < currentComp.Parts[i].Scenarios.Length; j++)
                {
                    calllist.Add(new HighscoreUpdateCall()
                    {
                        apicall = buildAPICallFromTaskName(currentComp.Parts[i].Scenarios[j].TaskName),
                        taskname = currentComp.Parts[i].Scenarios[j].TaskName
                    });

                    var docky = new DockPanel();
                    var parts = getAuthorIdAndWorkshopIdFromTaskName(currentComp.Parts[i].Scenarios[j].TaskName)?.Split(' ');
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

                    //play button at end of line
                    /**var parts = getAuthorIdAndWorkshopIdFromTaskName(currentComp.Parts[i].Scenarios[j].TaskName)?.Split(' ');
                    if (parts != null)
                    {
                        var btn = new Button()
                        {
                            Name = $"playbutton_{parts[0]}_{parts[1]}",
                            Width = 20,
                            Content = "▶️",
                        };
                        btn.Click += Button_Click;
                        docky.Children.Add(btn);
                    }
                    else Logger.log($"could not creat play button for '{currentComp.Parts[i].Scenarios[j].TaskName}'!");*/

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

            if (!string.IsNullOrEmpty(viewModel.klutchId)) launchCompetitionUpdates(calllist, updateCompetitionUIWithHighscore);
            else showMessageBox("please set 'klutchId' in Settings!");

            buildCompLeaderboard(calllist);
        }
        private void updateCompetitionUIWithHighscore(Task<HighscoreUpdateCall> call)
        {
            Trace.WriteLine("updating '" + call.Result.taskname + "'...");

            //get id name
            string textblockID = "";
            for (int i = 0; i < currentComp.Parts.Length; i++)
                for (int j = 0; j < currentComp.Parts[i].Scenarios.Length; j++)
                    if (currentComp.Parts[i].Scenarios[j].TaskName == call.Result.taskname)
                    {
                        textblockID = $"score_{i}_{j}";
                        this.Dispatcher.Invoke(() =>
                        {
                            //find texblock with matching ID
                            //var field = findPersonalCompetitionStatsScoreField(textblockID);
                            var field = compScoreFieldLookup.FirstOrDefault(f => f.Key == textblockID).Value;
                            if (field != null) field.Text = call.Result.highscore;
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

        //all competition contender scores
        private void buildCompLeaderboard(List<HighscoreUpdateCall> calllist)
        {
            List<Task> tasks = new List<Task>();

            foreach (var call in calllist)
            {
                var t = Task.Run(() =>
                {
                    for (int i = 0; i < currentComp.Parts.Length; i++)
                        for (int j = 0; j < currentComp.Parts[i].Scenarios.Length; j++)
                            if (currentComp.Parts[i].Scenarios[j].TaskName == call.taskname)
                                currentComp.Parts[i].Scenarios[j].leaderboard = new CompetitionTaskLeaderboard()
                                {
                                    TaskName = call.taskname,
                                    ResultsItem = APIStuff.getCompTaskLeaderboard(buildAPICallFromTaskName(call.taskname)).Result
                                };

                    Trace.WriteLine("added leaderboard for " + call.taskname);
                });
                tasks.Add(t);
            }
            Task.WaitAll(tasks.ToArray());
            Competition.buildCompContenders();
            buildCompLeaderboardToGUI();
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

        //other competition info
        private void addCompetitionTimerGUI(DockPanel boardDocky)
        {
            string? s = null;
            PartExpiraton nextEndingPart = null;

            for (int i = 0; i < currentComp.Parts.Length; i++)
            {
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

        //klutchId finder
        private void buildklutchIdCall()
        {
            if (!string.IsNullOrEmpty(viewModel.SteamLibraryPath) && Directory.Exists(viewModel.SteamLibraryPath))
            {
                var task = klutchIdFinder_Scenario.Text;
                var playername = klutchIdFinder_Username.Text;
                string call = buildAPICallFromTaskName(task);
                if (call != null) APIStuff.httpstuff(call).ContinueWith(item => findklutchId(item.Result.results, playername));
            }
        }
        private void findklutchId(Result[] results, string playername)
        {
            try
            {
                string? idFound = results.FirstOrDefault(r => r.username == playername)?.klutchId;
                this.Dispatcher.Invoke(() =>
                {
                    klutchIdFinderOutput.Visibility = Visibility.Visible;
                    klutchIdFinderOutputText.Text = idFound == null ? "user not found in leaderboard!" : idFound;
                });
            }
            catch (Exception e)
            {
                Logger.log($"FindklutchId Exception: User '{playername}' not found!");
                showMessageBox(e.Message);
            }
        }

        //nvenc screen recorder
        private void startRecording()
        {
            outputFileName = $"Replay_{DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss")}";
            if (viewModel.ReplaysPath != currentSettings.ReplaySavePath) currentSettings.ReplaySavePath = viewModel.ReplaysPath; //make sure it's up to date
            outputPath = currentSettings.ReplaySavePath != null ? currentSettings.ReplaySavePath : "./"; //if not set - save to app directory
            h264 = System.IO.Path.Combine(outputPath, outputFileName + ".264");

            var parts = viewModel.ReplayBufferSeconds.Split(' ');
            string seconds = parts.Length > 1 ? parts.Last().Substring(0, parts.Last().Length - 1) : parts[0].Substring(0, parts[0].Length - 1);

            var args = $"-d \\\\.\\DISPLAY1 -o {outputPath} -file {outputFileName} -r 24 -f 60 -s {seconds}";
            var argsArray = args.Split(' ');
            recorder = new ScreenCaptureNvenc(argsArray);
            if (recorder != null)
            {
                viewModel.IsRecording = true;
                replayBufferStatus_Output.Text = "Recording...";
                Trace.WriteLine("recording started!");
                Logger.log("recording started!");
            }
            /*string output = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".mp4");
            var snippet = FFmpeg.Conversions.FromSnippet.Convert(Resources.MkvWithAudio, output);
            IConversionResult result = await snippet.Start();*/

            recordStartButton.Content = "Stop";
        }
        private void stopRecording()
        {
            if (recorder != null) recorder.stop();
            viewModel.IsRecording = false;
            recordStartButton.Content = "Start";
            Trace.WriteLine("recording stopped!");
            Logger.log("stopped recording!");
        }
        private void saveReplay()
        {
            /*outputFileName = $"Replay_{DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss")}";
            outputPath = currentSettings.ReplaySavePath != null ? currentSettings.ReplaySavePath : "./";
            h264 = System.IO.Path.Combine(outputPath, outputFileName + ".264");*/

            recorder.saveReplay();

            Task.Run(() =>
            {
                this.Dispatcher.Invoke(() => replayBufferStatus_Output2.Text = "Saving Replay to " + mp4);
                Thread.Sleep(3000);
                this.Dispatcher.Invoke(() => replayBufferStatus_Output2.Text = "");
            });
        }
        public void takeScreenshot()
        {
            var bmpScreenshot = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            var gfxScreenshot = Graphics.FromImage(bmpScreenshot);
            gfxScreenshot.CopyFromScreen(Screen.PrimaryScreen.Bounds.X, Screen.PrimaryScreen.Bounds.Y, 0, 0, Screen.PrimaryScreen.Bounds.Size, CopyPixelOperation.SourceCopy);

            string savePath = null;
            if (!string.IsNullOrEmpty(viewModel.ScreenshotsPath) && Directory.Exists(viewModel.ScreenshotsPath))
            {
                savePath = viewModel.ScreenshotsPath;
            }
            else
            {
                savePath = "./Screenshots";
                Directory.CreateDirectory(savePath);
            }

            var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            string screenshotpath = $"{savePath}/Screenshot_{timestamp}.png";
            bmpScreenshot.Save(screenshotpath, System.Drawing.Imaging.ImageFormat.Png);

            Task.Run(() =>
            {
                this.Dispatcher.Invoke(() => autoRecordStatus_Output2.Text = "Saved Screenshot to " + screenshotpath);
                Thread.Sleep(3000);
                this.Dispatcher.Invoke(() => autoRecordStatus_Output2.Text = "");
            });
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
