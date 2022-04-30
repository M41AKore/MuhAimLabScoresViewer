using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data.SqlClient;
using System.Net.Http;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;
using System.Diagnostics;
using System.Threading;
using System.Runtime.InteropServices;
using System.Windows.Forms;
//using System.Drawing;
using MessageBox = System.Windows.MessageBox;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using Brushes = System.Windows.Media.Brushes;
using Color = System.Windows.Media.Color;
using Button = System.Windows.Controls.Button;
using System.Drawing;

namespace MuhAimLabScoresViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static string settingsPath = "./settings.xml";
        public static Settings currentSettings;
        public struct LeaderboardDisplay
        {
            public int place { get; set; }
            public string player { get; set; }
            public int score { get; set; }
            public int hit { get; set; }
            public int miss { get; set; }
            public string acc { get; set; }
        }
        public struct LevelAndWeapon
        {
            public string taskname;
            public string level;
            public string weapon;
        }
        public class BenchmarkDisplay
        {
            public string task { get; set; }
            public string highscore { get; set; }
            public string mythic { get; set; }
            public string immortal { get; set; }
            public string archon { get; set; }
            public string ethereal { get; set; }
            public string divine { get; set; }
        }
        public class HighscoreUpdateCall
        {
            public string apicall { get; set; }
            public string taskname { get; set; }
            public string highscore { get; set; }
            public string parentTaskName { get; set; }
        }

        public static string currentBenchmarkFilePath;
        public static string currentCompetitionFilePath;

        public static Competition currentComp;
        public static Benchmark currentBenchmark;

        Stopwatch timer = new Stopwatch();
        public class PartExpiraton
        {
            public Part part;
            public TimeSpan timeTillExpiration;
        }
        public DateTime nextEndingPart_DateTime_forTimer;
        public static MainWindow Instance { get; private set; }
        private ViewModel viewModel;

        Process recorderProcess = null;
        string outputFileName;
        string outputPath;
        string h264 = "";


        public MainWindow()
        {
            if (Instance == null) Instance = this;
            else
            {
                MessageBox.Show("there's already an instance running!");
                this.Close();
            }

            InitializeComponent();
            this.DataContext = new ViewModel();
            viewModel = this.DataContext as ViewModel;

            currentSettings = loadSettings();

            if (currentSettings.lastBenchmarkFile != null && File.Exists(currentSettings.lastBenchmarkFile)) HandleFile(currentSettings.lastBenchmarkFile);
            if (currentSettings.lastCompetitionFile != null && File.Exists(currentSettings.lastCompetitionFile)) HandleFile(currentSettings.lastCompetitionFile);
        }

        private void TaskButton_Click(object sender, RoutedEventArgs e)
        {
            TasksTab.Visibility = Visibility.Visible;
            TaskButton_BottomBorder.Visibility = Visibility.Hidden;

            BenchmarksTab.Visibility = Visibility.Collapsed;
            BenchmarkButton_BottomBorder.Visibility = Visibility.Visible;

            CompetitionsTab.Visibility = Visibility.Collapsed;
            CompetitionButton_BottomBorder.Visibility = Visibility.Visible;

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

            SettingsTab.Visibility = Visibility.Collapsed;
            SettingsButton_BottomBorder.Visibility = Visibility.Visible;

            this.Height = 830;
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

            SettingsTab.Visibility = Visibility.Collapsed;
            SettingsButton_BottomBorder.Visibility = Visibility.Visible;

            this.Height = 575;
            this.Width = 1125;
        }
        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            TasksTab.Visibility = Visibility.Collapsed;
            TaskButton_BottomBorder.Visibility = Visibility.Visible;

            BenchmarksTab.Visibility = Visibility.Collapsed;
            BenchmarkButton_BottomBorder.Visibility = Visibility.Visible;

            CompetitionsTab.Visibility = Visibility.Collapsed;
            CompetitionButton_BottomBorder.Visibility = Visibility.Visible;

            SettingsTab.Visibility = Visibility.Visible;
            SettingsButton_BottomBorder.Visibility = Visibility.Hidden;

            this.Height = 500;
            this.Width = 700;
        }
        private void TextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter) getLeaderboardFor((sender as System.Windows.Controls.TextBox).Text);
        }
        private void SteamLibraryInput_LostFocus(object sender, RoutedEventArgs e)
        {
            currentSettings.SteamLibraryPath = (sender as System.Windows.Controls.TextBox).Text;
        }
        private void klutchIdInput_LostFocus(object sender, RoutedEventArgs e)
        {
            currentSettings.klutchId = (sender as System.Windows.Controls.TextBox).Text;
        }
        private void Window_Closed(object sender, EventArgs e)
        {
            if (currentBenchmarkFilePath != null && File.Exists(currentBenchmarkFilePath)) currentSettings.lastBenchmarkFile = currentBenchmarkFilePath;
            if (currentCompetitionFilePath != null && File.Exists(currentCompetitionFilePath)) currentSettings.lastCompetitionFile = currentCompetitionFilePath;
            XmlSerializer.serializeToXml<Settings>(currentSettings, settingsPath);
        }
        private void DragDropInput_Benchmark_Drop(object sender, System.Windows.DragEventArgs e) => getFileDrop(e);
        private void DragDropInput_Competition_Drop(object sender, System.Windows.DragEventArgs e) => getFileDrop(e);
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string url = "https://go.aimlab.gg/v1/redirects?link=aimlab://workshop?id=2765722547&source=16BAE1433DACC70D&link=steam://rungameid/714010";

            System.Diagnostics.Process.Start("explorer", url);
        }
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            benchStacky.Children.Clear();
            if (currentBenchmarkFilePath != null) HandleFile(currentBenchmarkFilePath);
        }
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            CompetitionStacky.Children.Clear();
            CompInfoDocky.Children.Clear();
            myDocky.Children.Clear();
            if (currentCompetitionFilePath != null) HandleFile(currentCompetitionFilePath);
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
        private void Button_Click_4(object sender, RoutedEventArgs e) => stopRecording();
        private void Button_Click_5(object sender, RoutedEventArgs e) => buildklutchIdCall();
        private void Button_Click_3(object sender, RoutedEventArgs e) => startRecording();
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
            currentSettings.RecordingHotKey = e.Key;

            this.KeyUp += MainWindow_KeyDown; //only works when window in focus
        }     
        private void CheckBox_Checked(object sender, RoutedEventArgs e) => viewModel.BorderVisible = Visibility.Visible; //change to in xaml with converter
        private void CheckBox_Unchecked(object sender, RoutedEventArgs e) => viewModel.BorderVisible = Visibility.Collapsed;
        private void Button_Click_6(object sender, RoutedEventArgs e) => takeScreenshot();


     
        private static async Task<Item> httpstuff(string call)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    //f.e. "https://apiclient.aimlab.gg/leaderboards/scores?taskSlug=CsLevel.rA%20hebe.rA%20x%20Aim.R9GSEI&weaponName=Custom_rA100hz&map=42&mode=42&timeWindow=all");
                    HttpResponseMessage response = await client.GetAsync(call);
                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();
                    var item = JsonConvert.DeserializeObject<Item>(responseBody);
                    return item;
                }
                catch (HttpRequestException e)
                {
                    Console.WriteLine("\nException Caught!");
                    Console.WriteLine("Message :{0} ", e.Message);
                }
            }
            return null;
        }      
        private Settings loadSettings()
        {
            if (!File.Exists("./settings.xml")) File.WriteAllText("./settings.xml", "<Settings><SteamLibraryPath></SteamLibraryPath><klutchId></klutchId></Settings>");

            var settings = XmlSerializer.deserializeXml<Settings>("./settings.xml");
            SteamLibraryInput.Text = settings.SteamLibraryPath;
            klutchIdInput.Text = settings.klutchId;

            if (settings.RecordingHotKey != Key.None)
            {

            }

            return settings;
        }
        private void getFileDrop(System.Windows.DragEventArgs e)
        {
            if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop))
                foreach (string file in (string[])e.Data.GetData(System.Windows.DataFormats.FileDrop))
                    HandleFile(file);
        }
        private void HandleFile(string filepath)
        {
            timer.Start();
            string filename = filepath.Split('\\').Last();

            if (filename.ToLower().Contains("benchmark"))
            {
                var newbench = XmlSerializer.deserializeXml<Benchmark>(filepath);
                if (newbench != null)
                {                  
                    currentBenchmark = newbench;
                    Benchmark.addBenchmarkGUIHeaders(benchStacky);
                    Benchmark.addBenchmarkScores(benchStacky);
                    loadBenchmarkToGUI(newbench);
                    currentBenchmarkFilePath = filepath;
                }
            }

            if (filename.ToLower().Contains("competition") || filename.ToLower().Contains("comp"))
            {
                var newcomp = XmlSerializer.deserializeXml<Competition>(filepath);
                if (newcomp != null)
                {                
                    loadCompetitionToGUI(newcomp);
                    currentCompetitionFilePath = filepath;
                }
            }
        }      
        public static string buildAPICallFromTaskName(string task)
        {
            if (!Directory.Exists(currentSettings.SteamLibraryPath)) return null;

            DirectoryInfo[] dirs = new DirectoryInfo(currentSettings.SteamLibraryPath + @"\steamapps\workshop\content\714010").GetDirectories();
            foreach (var dir in dirs)
                foreach (var subdir in dir.GetDirectories())
                    if (subdir.Name == "Levels")
                        foreach (var file in subdir.GetDirectories()[0].GetFiles())
                            if (file.Name == "level.es3")
                            {
                                var content = File.ReadAllText(file.FullName);
                                if (content.Contains(task))
                                {
                                    var levelandweapon = collectLevelAndWeaponFromES3(content);
                                    return "https://apiclient.aimlab.gg/leaderboards/scores?taskSlug=" +
                                        levelandweapon.level + "&weaponName=" + levelandweapon.weapon + "&map=42&mode=42&timeWindow=all";
                                }
                            }

            return null;
        }
        private static LevelAndWeapon collectLevelAndWeaponFromES3(string filecontent)
        {
            /** didn't work out cause stupid escape characters and \t\r\t\t spam
             * 
            //Regex regex = new Regex("contentMetadata.+?(?=\"id)"); //"contentMetadata.+?(?=\"category)"

                                        var matches = regex.Matches(content);
                                        if(matches.Count > 0)
                                        {
                                            var str = matches[0].Value.ToString();
                                            //"contentMetadata" : {
				                                //   "id" : "CsLevel.rA hebe.f96a4d2c.R2GOSC",
				                                //    "label" : "rA Twoshot",

                                            Regex r2 = new Regex("d\" : \".+\"");
                                            //   d" : "CsLevel.rA hebe.f96a4d2c.R2GOSC"
                                            var ms = r2.Matches(str);
                                            if(ms.Count > 0)
                                            {
                                                var taskname = ms[0].Value.ToString().Substring(6, ms[0].Value.ToString().Length - 2);
                                                Console.WriteLine("found task '" + taskname + "'");
                                            }
                                        }
                                        */

            var start = filecontent.IndexOf("contentMetadata");
            var relevant = filecontent.Substring(start, filecontent.IndexOf("Skybox") - start);

            var lines = relevant.Split(new string[] { "\",", "{", "}" }, StringSplitOptions.RemoveEmptyEntries);

            var semirelevantlines = lines.Where(l => l.Contains("id\"") || l.Contains("label\"") || l.Contains("Weapon\"")).ToList();

            var idline = semirelevantlines.Where(l => l.Contains("id\"")).FirstOrDefault();
            var labelline = semirelevantlines.Where((l) => l.Contains("label\"")).FirstOrDefault();
            var weaponline = semirelevantlines.FirstOrDefault(l => l.Contains("Weapon\""));

            idline = uglyCleanup(idline);
            labelline = uglyCleanup(labelline);
            weaponline = uglyCleanup(weaponline);

            var result = new LevelAndWeapon()
            {
                taskname = labelline,
                level = idline.Replace(" ", "%20"),
                weapon = weaponline
            };

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
        public static SolidColorBrush getColorFromHex(string hexaColor)
        {
            return new SolidColorBrush(Color.FromArgb(255,
                    Convert.ToByte(hexaColor.Substring(1, 2), 16),
                    Convert.ToByte(hexaColor.Substring(3, 2), 16),
                    Convert.ToByte(hexaColor.Substring(5, 2), 16)));
        }
        private async void launchUpdates(List<HighscoreUpdateCall> calllist, Action<Task<HighscoreUpdateCall>> receiver)
        {
            timer.Restart();

            foreach (var call in calllist)
            {
                var t = Task.Run(async () => await getHighscore(call).ContinueWith(result => receiver(result))); // updateBenchmarkWithHighscore(result));
            }
        }

        //task leaderboard
        private void getLeaderboardFor(string taskname)
        {
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
                httpstuff(call).ContinueWith(item => populateleaderboard(item.Result.results));
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
                MessageBox.Show(e.Message);
            }
        }
        private void addTaskLeaderboardHeaders(StackPanel parent)
        {
            var headerDocky = new DockPanel()
            {
                Margin = new Thickness(5, 2, 5, 2),
                //Background = Brushes.Red
                Width = 380
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

            if (!string.IsNullOrEmpty(currentSettings.klutchId)) launchUpdates(calllist, updateBenchmarkWithHighscore);
            else MessageBox.Show("please set 'klutchId' in Settings!");
        }
        public static async Task<HighscoreUpdateCall> getHighscore(HighscoreUpdateCall call)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = await client.GetAsync(call.apicall);
                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();
                    Item item = JsonConvert.DeserializeObject<Item>(responseBody);

                    foreach (var r in item.results)
                        if (r.klutchId == currentSettings.klutchId)
                            call.highscore = r.score.ToString();
                }
                catch (HttpRequestException e)
                {
                    Console.WriteLine("\nException Caught!");
                    Console.WriteLine("Message :{0} ", e.Message);
                }
            }

            return call;
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
            Trace.WriteLine("updating '" + call.Result.taskname + "'...");
            var targetName = calculateUIScoreItemName(call);
            if (targetName != null)
            {
                this.Dispatcher.Invoke(() =>
                {
                    TextBlock tb = Benchmark.findBenchmarkScoreFieldWithName(targetName, benchStacky);
                    if (tb != null && int.TryParse(tb.Text, out int parentTaskScore) && parentTaskScore < int.Parse(call.Result.highscore))
                        tb.Text = call.Result.highscore;
                });
            }
        }

        //personal competition score display
        private void loadCompetitionToGUI(Competition comp)
        {
            currentComp = comp;
            var calllist = new List<HighscoreUpdateCall>();
            CompetitionStacky.Children.Clear();

            for (int i = 0; i < currentComp.Parts.Length; i++)
            {
                var stacky = new StackPanel();

                for (int j = 0; j < currentComp.Parts[i].Scenarios.Length; j++)
                {
                    calllist.Add(new HighscoreUpdateCall()
                    {
                        apicall = buildAPICallFromTaskName(currentComp.Parts[i].Scenarios[j].Name),
                        taskname = currentComp.Parts[i].Scenarios[j].Name
                    });

                    var docky = new DockPanel();
                    docky.Children.Add(new TextBlock()
                    {
                        Text = currentComp.Parts[i].Scenarios[j].Name,
                        Width = 220,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Background = Brushes.LightGray
                    });
                    docky.Children.Add(new TextBlock()
                    {
                        Name = $"score_{i}_{j}",
                        Width = 100,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Background = Brushes.White
                    });
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

            if (!string.IsNullOrEmpty(currentSettings.klutchId)) launchUpdates(calllist, updateCompetitionUIWithHighscore);
            else MessageBox.Show("please set 'klutchId' in Settings!");

            buildCompLeaderboard(calllist);
        }
        private void updateCompetitionUIWithHighscore(Task<HighscoreUpdateCall> call)
        {
            Trace.WriteLine("updating '" + call.Result.taskname + "'...");

            this.Dispatcher.Invoke(() =>
            {
                //get id name
                string textblockID = "";

                for (int i = 0; i < currentComp.Parts.Length; i++)
                    for (int j = 0; j < currentComp.Parts[i].Scenarios.Length; j++)
                        if (currentComp.Parts[i].Scenarios[j].Name == call.Result.taskname)
                            textblockID = $"score_{i}_{j}";


                //find texblock with matching ID
                var field = findPersonalCompetitionStatsScoreField(textblockID);
                if (field != null) field.Text = call.Result.highscore;

                return;

            });
        }
        public TextBlock findPersonalCompetitionStatsScoreField(string fieldname)
        {
            TextBlock result = null;

            foreach (var c1 in CompetitionStacky.Children)
                if (c1 is Border)
                    foreach (DockPanel docky in ((c1 as Border).Child as StackPanel).Children)
                        foreach (TextBlock field in docky.Children)
                            if (field != null && field.Name == fieldname)
                                return field;

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
                            if (currentComp.Parts[i].Scenarios[j].Name == call.taskname)
                                currentComp.Parts[i].Scenarios[j].leaderboard = new CompetitionTaskLeaderboard()
                                {
                                    TaskName = call.taskname,
                                    ResultsItem = getCompTaskLeaderboard(buildAPICallFromTaskName(call.taskname)).Result
                                };

                    Trace.WriteLine("added leaderboard for " + call.taskname);
                });
                tasks.Add(t);
            }
            Task.WaitAll(tasks.ToArray());
            buildCompContenders();
            buildCompLeaderboardToGUI();
        }
        private void buildCompContenders()
        {
            try
            {
                currentComp.competitionContenders = new List<CompetitionContender>();

                for (int i = 0; i < currentComp.Parts.Length; i++)
                {
                    for (int j = 0; j < currentComp.Parts[i].Scenarios.Length; j++)
                    {
                        foreach (var player in currentComp.Parts[i].Scenarios[j].leaderboard.ResultsItem.results)
                        {
                            // limit eligible scores to within time frame
                            var playdate = DateTime.UnixEpoch.AddSeconds(long.Parse(player.endedAt.Substring(0, player.endedAt.Length - 3)));
                            playdate = playdate.AddHours(12); //UTC-12 to UTC
                            var compPartEnddate = DateTime.Parse(currentComp.Parts[i].Enddate);
                            var compStartDate = DateTime.Parse(currentComp.Parts[i].Startdate);

                            if (playdate < compStartDate || playdate > compPartEnddate) continue;

                            var existingPlayer = currentComp.competitionContenders.FirstOrDefault(c => c.klutchId == player.klutchId);
                            if (existingPlayer == null) //create new
                            {
                                existingPlayer = new CompetitionContender()
                                {
                                    Name = player.username,
                                    klutchId = player.klutchId,
                                    mostRecentTimestamp = long.Parse(player.endedAt), //1 649 388 232 000
                                };
                                currentComp.competitionContenders.Add(existingPlayer);
                            }

                            //try to use newest name
                            if (existingPlayer.Name != player.username)
                                if (long.TryParse(player.endedAt, out long playTimestamp) && playTimestamp > existingPlayer.mostRecentTimestamp)
                                {
                                    existingPlayer.Name = player.username;
                                    existingPlayer.mostRecentTimestamp = playTimestamp;
                                }
                                    
                            if (existingPlayer.partResults == null)
                            {
                                existingPlayer.partResults = new CompetitorCompetitionPart[currentComp.Parts.Length];
                                for (int k = 0; k < currentComp.Parts.Length; k++)
                                {
                                    existingPlayer.partResults[k] = new CompetitorCompetitionPart()
                                    {
                                        taskResults = new CompetitionTaskResult[currentComp.Parts[k].Scenarios.Length]
                                    };
                                }
                            }

                            existingPlayer.partResults[i].taskResults[j] = new CompetitionTaskResult()
                            {
                                taskname = currentComp.Parts[i].Scenarios[j].Name,
                                score = player.score
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            //calculate points
            var allLeaderboards = new List<CompetitionTaskLeaderboard>();
            foreach (var part in currentComp.Parts)
                foreach (var scen in part.Scenarios)
                {
                    scen.leaderboard.calculateMeanTop20OfScenarios();
                    allLeaderboards.Add(scen.leaderboard);
                }

            currentComp.competitionContenders.ForEach(c => c.calculateScorePoints(allLeaderboards)); //points results from top20 score deviation
            currentComp.competitionContenders = currentComp.competitionContenders.OrderByDescending(c => c.totalPoints).ToList();            
        }
        private void buildCompLeaderboardToGUI()
        {
            Competition_Title.Text = currentComp.Title;
            Competition_Title.FontWeight = FontWeights.Bold;
            CompInfoDocky.Children.Clear();
            myDocky.Children.Clear();
            addOtherCompetitionGUI(CompInfoDocky); //timer
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
                Width = 220,
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
                        Text = currentComp.Parts[i].Scenarios[j].Name,
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
        private async Task<Item> getCompTaskLeaderboard(string apicall)
        {
            Item item = null;
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = await client.GetAsync(apicall);
                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();
                    item = JsonConvert.DeserializeObject<Item>(responseBody);
                }
                catch (HttpRequestException e)
                {
                    Console.WriteLine("\nException Caught!");
                    Console.WriteLine("Message :{0} ", e.Message);
                }
            }
            return item;
        }
        
        //other competition info
        private void addOtherCompetitionGUI(DockPanel boardDocky)
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

            if (s == null && nextEndingPart == null) s = "None";
            else
            {
                if (nextEndingPart.timeTillExpiration > TimeSpan.FromHours(48))
                {
                    lbl_endson.Text = "ends on:";
                    lbl_partendtimer.Text = $"{nextEndingPart.part.Enddate} (UTC)";
                }
                else
                {
                    lbl_endson.Text = "ends in:";
                    int hours = nextEndingPart.timeTillExpiration.Days > 0 ? nextEndingPart.timeTillExpiration.Hours + 24 : nextEndingPart.timeTillExpiration.Hours;
                    lbl_partendtimer.Text = $"{hours}:{nextEndingPart.timeTillExpiration.Minutes}:{nextEndingPart.timeTillExpiration.Seconds}";

                    //timer
                    nextEndingPart_DateTime_forTimer = DateTime.Parse(nextEndingPart.part.Enddate);
                    var countdownClock = Task.Run(() =>
                    {
                        while (!updateCountdownTimer())
                            Thread.Sleep(1000);
                    });
                }
            }
            lbl_activepart.Text = s;
        }
        private bool updateCountdownTimer()
        {
            var isOver = false;
            this.Dispatcher.Invoke(() =>
            {
                var timeLeft = nextEndingPart_DateTime_forTimer - DateTime.UtcNow;
                if (timeLeft > TimeSpan.Zero)
                {
                    int hoursTotal = timeLeft.Days > 0 ? timeLeft.Hours + 24 : timeLeft.Hours;
                    string hours = hoursTotal < 10 ? $"0{hoursTotal}" : hoursTotal.ToString();
                    string minutes = timeLeft.Minutes < 10 ? $"0{timeLeft.Minutes}" : timeLeft.Minutes.ToString();
                    string seconds = timeLeft.Seconds < 10 ? $"0{timeLeft.Seconds}" : timeLeft.Seconds.ToString();
                    lbl_partendtimer.Text = $"{hours}:{minutes}:{seconds}";
                    if (hoursTotal < 1) lbl_partendtimer.Foreground = Brushes.Red;
                }
                else isOver = true;
            });
            return isOver;
        }

        //klutchId finder
        private void buildklutchIdCall()
        {
            string providedName = klutchIdFinder_Username.Text;
            string providedScenario = klutchIdFinder_Scenario.Text;

            string call = buildAPICallFromTaskName(providedScenario);
            if (call != null) httpstuff(call).ContinueWith(item => findklutchId(item.Result.results, providedName));
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
                MessageBox.Show(e.Message);
            }
        }

        //nvenc screen recorder
        private void startRecording()
        {
            var proc = new ProcessStartInfo();
            proc.WindowStyle = ProcessWindowStyle.Hidden;
            proc.FileName = @"C:\Users\Kore\Downloads\NvEncSharp-master\src\NvEncSharp.Sample.ScreenCapture\bin\Debug\NvEncSharp.Sample.ScreenCapture.exe";

            outputFileName = $"Replay_{DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss")}";
            outputPath = "D:/ballertest/";
            h264 = System.IO.Path.Combine(outputPath, outputFileName + ".264");

            var parts = viewModel.ReplayBufferSeconds.Split(' ');
            string seconds = parts.Length > 1 ? parts.Last().Substring(0, parts.Last().Length - 1) : parts[0].Substring(0, parts[0].Length - 1);
            var args = $"-d \\\\.\\DISPLAY1 -o {outputPath} -file {outputFileName} -r 24 -f 60 -s {seconds}";
            proc.Arguments = args;
            recorderProcess = Process.Start(proc);

            if (recorderProcess != null)
            {
                viewModel.IsRecording = true;

                recordStartButton.Visibility = Visibility.Collapsed;
                replayBufferStatus_Output.Text = "Recording...";
                Trace.WriteLine("recording started!");
            }
            /*string output = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".mp4");
            var snippet = FFmpeg.Conversions.FromSnippet.Convert(Resources.MkvWithAudio, output);
            IConversionResult result = await snippet.Start();*/
        }
        private void stopRecording()
        {
            if (recorderProcess != null) recorderProcess.CloseMainWindow();
            Trace.WriteLine("recording stopped!");
            replayBufferStatus_Output.Text = "Saving...";

            viewModel.IsRecording = false;
            recordStartButton.Visibility = Visibility.Visible;

            Thread.Sleep(1000); // ehm...

            Console.WriteLine("converting to mp4...");
            ProcessStartInfo proc = new ProcessStartInfo();
            proc.WindowStyle = ProcessWindowStyle.Hidden;
            proc.FileName = @"C:\windows\system32\cmd.exe";
            string mp4 = h264.Substring(0, h264.Length - 3) + "mp4";
            proc.Arguments = $"/c D:\\ballertest\\ffmpeg-master\\bin\\ffmpeg.exe -y -i {h264} -c copy {mp4}"; //-loglevel quiet -nostats
            var p = Process.Start(proc);
            //p.Exited += P_Exited; ;
            Trace.WriteLine("wrapped in mp4 container!");
        }
        private void P_Exited(object? sender, EventArgs e)
        {
            File.Delete(System.IO.Path.Combine(outputPath, $"{outputFileName}.264"));
            Console.WriteLine("deleted h264 file!");
        }
        private void MainWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == currentSettings.RecordingHotKey)
            {
                MessageBox.Show("recording hotkey pressed!");
            }
        }
        private void takeScreenshot()
        {
            var bmpScreenshot = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            var gfxScreenshot = Graphics.FromImage(bmpScreenshot);
            gfxScreenshot.CopyFromScreen(Screen.PrimaryScreen.Bounds.X, Screen.PrimaryScreen.Bounds.Y, 0, 0, Screen.PrimaryScreen.Bounds.Size, CopyPixelOperation.SourceCopy);

            var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            bmpScreenshot.Save($"D:/ballertest/Screenshot{timestamp}.png", System.Drawing.Imaging.ImageFormat.Png);
        }
    }
}
