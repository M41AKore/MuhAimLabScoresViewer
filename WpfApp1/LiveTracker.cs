using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Threading;
using static MuhAimLabScoresViewer.MainWindow;
using static MuhAimLabScoresViewer.Helper;
using static MuhAimLabScoresViewer.ObjectsAndStructs;
using OxyPlot.Series;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Wpf;
using static MuhAimLabScoresViewer.APIStuff;
using static MuhAimLabScoresViewer.SheetsInteraction;
using System.Reflection;
using System.Runtime.CompilerServices;
using Task = System.Threading.Tasks.Task;

namespace MuhAimLabScoresViewer
{
    public class LiveTracker
    {
        const UInt32 WM_KEYDOWN = 0x0100;
        const int WM_SYSKEYDOWN = 0x0104;
        const int VK_F5 = 0x74;

        [DllImport("user32.dll")]
        static extern bool PostMessage(IntPtr hWnd, UInt32 Msg, int wParam, int lParam);

        public static SQLiteConnection sqlite;
        public static string LocalDBFile;
        private static FileSystemWatcher watcher;

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void keybd_event(uint bVk, uint bScan, uint dwFlags, uint dwExtraInfo);

        public static ScenarioHistory currentScenario;

        static bool done = false;

        private static async Task hotkeyCooldown()
        {
            await Task.Delay(2000);
            done = false;
        }

        public static void testOBSHotkey()
        {
            doHighscoreRecordingStuff(1337, true);
        }

        public static void simulateKeyPress(string hotkey)
        {
            if (!done)
            {
                done = true;
                hotkeyCooldown();
            }
            else return;

            if (string.IsNullOrEmpty(hotkey)) return;

            Trace.WriteLine("simulateKeyPress()");

            //keybd_event((uint)VirtualKeysDictionary.getVirtualKey(hotkey), 0x45, 0, (uint)IntPtr.Zero);
            //keybd_event((uint)VirtualKeysDictionary.getVirtualKey(hotkey), 0, 0, 0);

            Process[] processes = Process.GetProcessesByName("obs64"); //does this get streamlabs? is obs studio a different thing?
            if (processes.Length > 0)
            {
                Trace.WriteLine("found obs64 !");

                foreach (Process proc in processes)
                    PostMessage(proc.MainWindowHandle, WM_SYSKEYDOWN, VirtualKeysDictionary.getVirtualKey(hotkey), 0);
            }

            //it seems this is also due to Windows shittery and OBS being unresponsive or so, let's try this ig

            //OBS.libobs.obs_property_button_clicked()

            /*await Task.Run(() =>
            {
                for (int i = 0; i < 10; i++)
                {
                    keybd_event((uint)VirtualKeysDictionary.getVirtualKey(hotkey), 0, 0, 0);
                    Thread.Sleep(100);
                }           
            });*/
        }

        private static bool readKlutchBytes()
        {
            //C:\Users\Kore\AppData\LocalLow\Statespace\aimlab_tb\klutch.bytes // "SQLite format 3"

            //if (!checkDBfile()) return false; //file is apparently always in use, but reading it seems to work just fine anyways
            try
            {
                Logger.log("reading db file...");
                sqlite = new SQLiteConnection($"Data Source={LocalDBFile}"); //;New=False;

                detectNewHighscore(); //for autorecord

                if (viewModel.LiveTrackerEnabled)
                {
                    createLiveTrackerGUI();
                    generateResultView(MainWindow.Instance.windowTabs[5] as LiveResultTab);
                }


                return true;

            }
            catch (Exception ex)
            {
                Logger.log("exception thrown when trying to read database file!" + Environment.NewLine + ex.Message);
            }

            return false;
        }

        public static async void generateResultView(LiveResultTab tab)
        {
            sqlite = new SQLiteConnection($"Data Source={LocalDBFile}"); //;New=False;

            var results = selectQuery($"SELECT * FROM TaskData ORDER BY createDate DESC");
            var rows = results.Select();

            var last = rows.FirstOrDefault();
            if (last != null)
            {
                string taskName = last["taskName"].ToString();
                string workshopid = last["workshopId"].ToString();
                string score = last["score"].ToString();
                DateTime timestamp = DateTime.Parse(last["createDate"].ToString());

                string performance = last["performance"].ToString();
                int accIndex = performance.IndexOf("accTotal");
                var parts = performance.Substring(accIndex, performance.Length - accIndex).Split('"');
                string accuracy = parts[1].Replace(':', ' ').Replace(',', ' ').Trim() + "%";


                if (!string.IsNullOrEmpty(viewModel.klutchId))
                {
                    var r = await runGetTaskResultsQuery(viewModel.klutchId);
                    if(r != null) 
                    {
                        
                    }
                }
                else MainWindow.Instance.showMessageBox("please set 'klutchId' in Settings!");


                ;
                MainWindow.Instance.Dispatcher.Invoke(() =>
                {
                    if (tab != null)
                    {
                        tab.ResultGraphTitle.Content = taskName;
                        tab.Result_Score.Text = score;
                        tab.Result_Accuracy.Text = accuracy;

                        tab.resultgraphStacky.Children.Clear();

                        /**scenario.Plays = scenario.Plays.OrderBy(p => p.Date).ToList(); //left to right graph

                        //scores plot
                        FunctionSeries fs = new FunctionSeries();
                        for (int i = 0; i < scenario.Plays.Count; i++)
                            fs.Points.Add(new OxyPlot.DataPoint(i + 1, int.Parse(scenario.Plays[i].Score)));


                        //median plot
                        FunctionSeries fs2 = new FunctionSeries();
                        fs2.Color = OxyColor.FromArgb(255, 255, 0, 0);

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
                            else medians.Add(double.Parse(group[(i + 1) / 2].Score));
                        }

                        for (int i = 0; i < medians.Count; i++)
                            fs2.Points.Add(new OxyPlot.DataPoint(i + 1, medians[i]));

                        PlotView pv = new PlotView();
                        pv.Height = 600;
                        pv.Width = 600;
                        PlotModel n = new PlotModel();
                        n.Series.Add(fs);
                        n.Series.Add(fs2);
                        n.Axes.Add(new LinearAxis()
                        {
                            Position = AxisPosition.Bottom,
                            Title = "Plays",
                            MajorGridlineStyle = OxyPlot.LineStyle.Solid,
                            MinorGridlineStyle = OxyPlot.LineStyle.None,
                        });
                        n.Axes.Add(new LinearAxis());
                        pv.Model = n;

                        var tab = MainWindow.Instance.windowTabs[4] as LiveTrackerTab;
                        if (tab == null) return;

                        if (tab.trackergraphStacky.Children.Count > 0) tab.trackergraphStacky.Children.RemoveAt(0);
                        tab.trackergraphStacky.Children.Add(pv);

                        //other info section
                        var orderedPlays = scenario.Plays.OrderBy(p => int.Parse(p.Score)).ToArray();
                        tab.txt_TrackerPlays.Text = scenario.Plays.Count.ToString();
                        tab.txt_TrackerHighscore.Text = orderedPlays.Last().Score.ToString();
                        tab.txt_TrackerAverage.Text = scenario.Plays.Average(p => int.Parse(p.Score)).ToString("#.##");

                        //median
                        if (scenario.Plays.Count % 2 == 0) //if even, calculate median at half
                        {
                            int halfPlusOne = int.Parse(orderedPlays[scenario.Plays.Count / 2].Score);
                            int halfMinusOne = int.Parse(orderedPlays[(scenario.Plays.Count / 2) - 1].Score);
                            double median = ((double)(halfPlusOne + halfMinusOne) / 2);
                            tab.txt_TrackerMedian.Text = median.ToString();
                        }
                        else
                        {
                            tab.txt_TrackerMedian.Text = orderedPlays[scenario.Plays.Count / 2].Score.ToString();
                        }*/

                    }
                });
            }
        }

        public static ScenarioHistory determineCurrentScenario()
        {
            ScenarioHistory current = null;
            sqlite = new SQLiteConnection($"Data Source={LocalDBFile}"); //;New=False;


            // TODO: change the session length behaviour to "max pause", "min time between session" or "session cutoff"
            // where then if the time between two results of the same taskid is greater than that time, see as separate sessions

            var dateString = DateTime.Now.AddMinutes(-viewModel.LiveTrackerMinutes).ToString("yyyy-MM-dd HH:mm:ss");
            var results = selectQuery($"SELECT * FROM TaskData WHERE createDate >= '{dateString}' ORDER BY createDate DESC");
            var rows = results.Select();

            //this is done in select statement now
            //var compareSpan = TimeSpan.FromMinutes(MainWindow.viewModel.LiveTrackerMinutes);
            //var relevant = rows.Where(r => DateTime.TryParse(r["createDate"].ToString(), out DateTime date) && DateTime.UtcNow - date < compareSpan).ToList();
            //var orderedRelevant = relevant.OrderByDescending(r => DateTime.Parse(r["createDate"].ToString())).ToList();

            var last = rows.FirstOrDefault();
            if (last != null)
            {
                current = new ScenarioHistory()
                {
                    Identification = last["taskName"].ToString(),
                    Name = null, //last["name"].ToString(),
                    WorkshopId = last["workshopId"].ToString(),
                    Plays = new List<Play>() { new Play()
                        {
                            DateString = last["createDate"].ToString(),
                            Date = DateTime.Parse(last["createDate"].ToString()),
                            Score = last["score"].ToString(),
                            //Accuracy = last["accuracy"].ToString(), // would have to parse ["performance"] for 
                        }
                },
                };

                for (int i = 0; i < rows.Length; i++)
                {
                    if (rows[i] == last) continue;

                    if (rows[i]["taskName"].ToString() == last["taskName"].ToString())
                    {
                        current.Plays.Add(new Play()
                        {
                            DateString = rows[i]["createDate"].ToString(),
                            Date = DateTime.Parse(rows[i]["createDate"].ToString()),
                            Score = rows[i]["score"].ToString(),
                            //Accuracy = relevant[i]["accuracy"].ToString(), //see comment above 
                        });
                    }
                    else
                    {
                        break;
                    }
                }
            }

            return current;
        }

        public static void createLiveTrackerGUI()
        {
            currentScenario = determineCurrentScenario();
            if (currentScenario != null && !string.IsNullOrEmpty(currentScenario.Identification) && !string.IsNullOrEmpty(currentScenario.WorkshopId)) 
                currentScenario.Name = getTaskNameFromLevelID(currentScenario.Identification, currentScenario.WorkshopId);

            MainWindow.Instance.Dispatcher.Invoke(() =>
                {
                    var tab = MainWindow.Instance.windowTabs[4] as LiveTrackerTab;
                    if (currentScenario == null) tab.TrackerGraphTitle.Content = "No plays found yet in given Session window!";
                    else
                    {
                        tab.TrackerGraphTitle.Content = !string.IsNullOrEmpty(currentScenario.Name) ? currentScenario.Name : currentScenario.Identification;
                        tab.trackergraphStacky.Children.Clear();
                        createDataPoints(currentScenario);
                    }
                });
        }

        private static void createDataPoints(ScenarioHistory scenario)
        {
            scenario.Plays = scenario.Plays.OrderBy(p => p.Date).ToList(); //left to right graph

            //scores plot
            FunctionSeries fs = new FunctionSeries();
            for (int i = 0; i < scenario.Plays.Count; i++)
                fs.Points.Add(new OxyPlot.DataPoint(i + 1, int.Parse(scenario.Plays[i].Score)));


            //median plot
            FunctionSeries fs2 = new FunctionSeries();
            fs2.Color = OxyColor.FromArgb(255, 255, 0, 0);

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
                else medians.Add(double.Parse(group[(i + 1) / 2].Score));
            }

            for (int i = 0; i < medians.Count; i++)
                fs2.Points.Add(new OxyPlot.DataPoint(i + 1, medians[i]));

            PlotView pv = new PlotView();
            pv.Height = 600;
            pv.Width = 600;
            PlotModel n = new PlotModel();
            n.Series.Add(fs);
            n.Series.Add(fs2);
            n.Axes.Add(new LinearAxis()
            {
                Position = AxisPosition.Bottom,
                Title = "Plays",
                MajorGridlineStyle = OxyPlot.LineStyle.Solid,
                MinorGridlineStyle = OxyPlot.LineStyle.None,
            });
            n.Axes.Add(new LinearAxis());
            pv.Model = n;

            var tab = MainWindow.Instance.windowTabs[4] as LiveTrackerTab;
            if (tab == null) return;

            if (tab.trackergraphStacky.Children.Count > 0) tab.trackergraphStacky.Children.RemoveAt(0);
            tab.trackergraphStacky.Children.Add(pv);

            //other info section
            var orderedPlays = scenario.Plays.OrderBy(p => int.Parse(p.Score)).ToArray();
            tab.txt_TrackerPlays.Text = scenario.Plays.Count.ToString();
            tab.txt_TrackerHighscore.Text = orderedPlays.Last().Score.ToString();
            tab.txt_TrackerAverage.Text = scenario.Plays.Average(p => int.Parse(p.Score)).ToString("#.##");

            //median
            if (scenario.Plays.Count % 2 == 0) //if even, calculate median at half
            {
                int halfPlusOne = int.Parse(orderedPlays[scenario.Plays.Count / 2].Score);
                int halfMinusOne = int.Parse(orderedPlays[(scenario.Plays.Count / 2) - 1].Score);
                double median = ((double)(halfPlusOne + halfMinusOne) / 2);
                tab.txt_TrackerMedian.Text = median.ToString();
            }
            else
            {
                tab.txt_TrackerMedian.Text = orderedPlays[scenario.Plays.Count / 2].Score.ToString();
            }
        }

        private static void updateLiveTrackerGUI()
        {
            // TODO - don't everytime just create everything from scratch mebi
        }

        private static void detectNewHighscore()
        {
            var lastResult = selectQuery("SELECT * FROM TaskData ORDER BY createDate DESC LIMIT 1"); // );
            var rows = lastResult.Select();
            for (int i = 0; i < rows.Length; i++)
            {
                Logger.log($"last result for '{rows[i]["taskName"]}' = '{rows[i]["score"]}'");

                //string call = buildAPICallFromTaskID(rows[i]["taskName"].ToString());
                var stuff = Helper.getLevelAndWeaponForTask(rows[i]["taskName"].ToString());
                compareToHighscore(stuff, rows[i]["score"].ToString(), rows[i]["performance"].ToString());
            }
        }

        public static async void compareToHighscore(LevelAndWeapon stuff, string achievedScore, string performanceString)
        {
            if (stuff == null) return;
            var r = await APIStuff.runLeaderboardQuery(stuff.level.Replace("%20", " "), stuff.weapon, 0, 100);
            if (r == null) return;

            var playerResult = r.aimlab.leaderboard.data.FirstOrDefault(r => r.user_id == SettingsTab.currentSettings.klutchId);
            if (playerResult != null)
            {
                Logger.log($"api highscore = '{playerResult.score}', this score = '{achievedScore}'");
                if (int.TryParse(achievedScore, out int newscore) && newscore >= playerResult.score)
                {
                    Logger.log("highscore detected!");

                    //determine if highscore was set with this attempt
                    var performanceJson = JsonConvert.DeserializeObject<Performance>(performanceString);
                    if (performanceJson != null)
                    {
                        if (performanceJson.hitsTotal != playerResult.shots_hit
                                //|| performanceJson.missesTotal != playerResult.shots_missed
                                || performanceJson.targetsTotal != playerResult.targets)
                        {
                            //if any of these values is different, this is most likely a new result
                            //potentially still record duplicate score attempts
                            if (newscore == playerResult.score && viewModel.AutoRecordDuplicates)
                            {
                                doHighscoreRecordingStuff(newscore);
                            }
                            else if (newscore > playerResult.score) //OR, result hasn't made it to API yet, so only record if higher
                            {
                                doHighscoreRecordingStuff(newscore);
                            }
                        }
                        else
                        {
                            //last registered score seems to have set new API highscore, therefore we save replay
                            doHighscoreRecordingStuff(newscore);
                        }
                    }
                }
            }
        }

        private static async Task doHighscoreRecordingStuff(int score, bool test = false)
        {
            MainWindow.Instance.displayAutoRecordStatusMessage($"New highscore of '{score}' detected!");
            if (viewModel.onSaveReplayTakeScreenshot)
            {
                var tab = MainWindow.Instance.windowTabs[6] as SettingsTab;
                if (tab != null) tab.takeScreenshot();
            }
            simulateKeyPress(viewModel.OBS_Key);

            int delaySeconds = viewModel.VODrenameDelay > 3 ? viewModel.VODrenameDelay : 3;
            await Task.Delay(delaySeconds * 1000); //min 3s

            if (!string.IsNullOrEmpty(viewModel.OBSoutputDirectory)) // !string.IsNullOrEmpty(viewModel.HighscoreVODname) && 
            {
                var vodDir = new DirectoryInfo(viewModel.OBSoutputDirectory);
                if (vodDir.Exists)
                {
                    Trace.WriteLine("directory exists");

                    var files = vodDir.GetFiles();
                    var bynewest = files.OrderByDescending(f => f.LastWriteTime);

                    var newest = bynewest.FirstOrDefault(f => f.Extension == ".mp4" || f.Extension == ".mkv");
                    if (newest != null)
                    {
                        Trace.WriteLine("most recent video file is " + newest.Name);
                        //System.IO.File.Move(newest.FullName, newest.FullName);

                        if (!test && newest.LastWriteTime < DateTime.Now - TimeSpan.FromSeconds(delaySeconds + 1)) //max 1s before save recording was initiated
                        {
                            Logger.log($"Most recent video file '{newest.Name}' was found to be created before this vod saving was initiated!");
                            return;
                        }

                        var lastResult = selectQuery("SELECT * FROM TaskData ORDER BY createDate DESC LIMIT 1"); // );
                        var rows = lastResult.Select();

                        Trace.WriteLine("lastresult is " + rows[0]["taskName"] + ", " + rows[0]["score"]);
                        
                        if(test || score == int.Parse(rows[0]["score"].ToString()))
                        {
                            var taskname = Helper.getTaskNameFromLevelID(rows[0]["taskName"].ToString(), rows[0]["workshopId"].ToString());

                            string newName = $"{taskname} {rows[0]["score"]}{newest.Extension}";
                            Trace.WriteLine($"renaming {newest.Name} to {newName}");
                            newest.MoveTo(newest.Directory.FullName + "\\" + newName);

                            if(File.Exists(newest.Directory.FullName + "\\" + newName))
                            {
                                MainWindow.Instance.displayAutoRecordStatusMessage($"Renamed VOD to '{newName}'!");
                            }
                            
                        }
                    }
                }
            }
        }

        private bool checkDBfile()
        {
            //MemoryStream inMemoryCopy = null;

            try
            {
                using (FileStream fs = File.Open(LocalDBFile, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                {
                    fs.Close();
                    return true;
                }
                // The file is not locked 

                /*inMemoryCopy = new MemoryStream();
                using (FileStream fs = File.OpenRead(LocalDBFile))
                {
                    fs.CopyTo(inMemoryCopy);
                }*/
            }
            catch (Exception e)
            {
                // The file is locked 
                //try again on next check
                Logger.log("db file was in use");
                Logger.log(e.Message.ToString());
                return false;
            }

            //return inMemoryCopy;
            return false;
        }

        public static DataTable selectQuery(string query)
        {
            // selectQuery("SELECT name FROM sqlite_master WHERE type = 'table' ORDER BY 1");
            //var dt = selectQuery("SELECT * FROM TaskData WHERE performanceClass = 'CSTask'");
            SQLiteDataAdapter ad;
            DataTable dt = new DataTable();

            try
            {
                SQLiteCommand cmd;
                sqlite.Open();  //Initiate connection to the db
                cmd = sqlite.CreateCommand();
                cmd.CommandText = query;  //set the passed query
                ad = new SQLiteDataAdapter(cmd);
                ad.Fill(dt); //fill the datasource
            }
            catch (SQLiteException ex)
            {
                MainWindow.Instance.showMessageBox(ex.Message);
                //Add your exception code here.
            }
            sqlite.Close();
            return dt;
        }

        public static void setupFileWatch()
        {
            try
            {
                watcher = new FileSystemWatcher(LocalDBFile.Replace("klutch.bytes", string.Empty));

                watcher.NotifyFilter = NotifyFilters.Attributes
                                         | NotifyFilters.CreationTime
                                         | NotifyFilters.DirectoryName
                                         | NotifyFilters.FileName
                                         | NotifyFilters.LastAccess
                                         | NotifyFilters.LastWrite
                                         | NotifyFilters.Security
                                         | NotifyFilters.Size;

                watcher.Changed += OnChanged;
                watcher.Created += OnCreated;
                watcher.Deleted += OnDeleted;
                watcher.Renamed += OnRenamed;
                watcher.Error += OnError;

                watcher.Filter = "*.bytes";
                watcher.IncludeSubdirectories = false;
                watcher.EnableRaisingEvents = true;
                Logger.log("setup db file watcher!");
            }
            catch (Exception ex)
            {
                Logger.log(ex.Message);
            }
        }

        public static void removeFileWatch()
        {
            if (watcher != null)
            {
                try
                {
                    watcher.Dispose();
                    Logger.log("disposed db file watcher!");
                }
                catch (Exception ex)
                {
                    Logger.log(ex.Message);
                }
            }
        }

        private static void OnChanged(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType != WatcherChangeTypes.Changed) return;

            var t = System.Threading.Tasks.Task.Run(() =>
                {
                    while (!readKlutchBytes())
                    {
                        Thread.Sleep(1000);
                    }
                });
        }
        private static void OnCreated(object sender, FileSystemEventArgs e)
        {
            string value = $"Created: {e.FullPath}";
            Console.WriteLine(value);
        }
        private static void OnDeleted(object sender, FileSystemEventArgs e) =>
                Console.WriteLine($"Deleted: {e.FullPath}");
        private static void OnRenamed(object sender, RenamedEventArgs e)
        {
            Console.WriteLine($"Renamed:");
            Console.WriteLine($"    Old: {e.OldFullPath}");
            Console.WriteLine($"    New: {e.FullPath}");
        }
        private static void OnError(object sender, ErrorEventArgs e) => PrintException(e.GetException());
        private static void PrintException(Exception? ex)
        {
            if (ex != null)
            {
                Console.WriteLine($"Message: {ex.Message}");
                Console.WriteLine("Stacktrace:");
                Console.WriteLine(ex.StackTrace);
                Console.WriteLine();
                PrintException(ex.InnerException);
            }
        }

    }
}
