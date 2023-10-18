using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static MuhAimLabScoresViewer.APIStuff;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;
using Button = System.Windows.Controls.Button;
using System.Threading;
using System.Drawing;

namespace MuhAimLabScoresViewer
{
    /// <summary>
    /// Interaction logic for SettingsTab.xaml
    /// </summary>
    public partial class SettingsTab : Page
    {
        ScreenCaptureNvenc recorder = null;
        string outputFileName = "";
        string outputPath = "";
        string h264 = "";
        string mp4 = "";

        KeyboardHook hook = new KeyboardHook();
        private bool registeredHotkey = false;

        public static string settingsPath = "./settings.xml";
        public static Settings? currentSettings;
        private ViewModel viewModel;

        private bool klutchfinding = false;

        public SettingsTab()
        {
            InitializeComponent();
            DataContext = MainWindow.viewModel;
            viewModel = MainWindow.viewModel;
        }

        private void clickTestOBSHotkey(object sender, RoutedEventArgs e) => LiveTracker.testOBSHotkey();
        private void Button_Click_5(object sender, RoutedEventArgs e) => buildklutchIdCall();
        //klutchId finder
        private async void buildklutchIdCall()
        {
            if (klutchfinding) return;
            if (!string.IsNullOrEmpty(viewModel.SteamLibraryPath) && Directory.Exists(viewModel.SteamLibraryPath))
            {
                viewModel.KlutchIdFinderLoading = Visibility.Visible;
                klutchIdFinderOutput.Visibility = Visibility.Collapsed;
                klutchfinding = true;

                var task = klutchIdFinder_Scenario.Text;
                var playername = klutchIdFinder_Username.Text;
                //string call = buildAPICallFromTaskName(task);
                var stuff = Helper.getLevelAndWeaponForTask(task);
                if (stuff != null)
                {
                    string levelId = stuff.level.Replace("%20", " ");
                    int start = 0;
                    int end = 100;

                    LeaderboardResult r = await runLeaderboardQuery(levelId, stuff.weapon, start, end);
                    while (klutchfinding && r != null && await findUser(r, playername) == null && start < r.aimlab.leaderboard.metadata.totalRows)
                    {
                        start += 100;
                        end += 100;
                        r = await runLeaderboardQuery(levelId, stuff.weapon, 0, 100);
                    }
                    findklutchId(r, playername);
                }
            }
        }

        private async Task<string> findUser(LeaderboardResult result, string username)
        {
            string foundKlutchId = null;
            foundKlutchId = result.aimlab.leaderboard.data.FirstOrDefault(r => r.username == username)?.user_id;
            return foundKlutchId;
        }

        private void findklutchId(LeaderboardResult result, string? playername)
        {
            try
            {
                string? idFound = result.aimlab.leaderboard.data.FirstOrDefault(r => r.username == playername)?.user_id;
                this.Dispatcher.Invoke(() =>
                {
                    klutchIdFinderOutput.Visibility = Visibility.Visible;
                    klutchIdFinderOutputText.Text = idFound == null ? "user not found in leaderboard!" : idFound;
                    viewModel.KlutchIdFinderLoading = Visibility.Collapsed;
                    klutchfinding = false;
                });
            }
            catch (Exception e)
            {
                Logger.log($"FindklutchId Exception: User '{playername}' not found!");
                MainWindow.Instance.showMessageBox(e.Message);
            }
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
        public void registerRecordingHotkey(Settings settings)
        {
            // register the event that is fired after the key press.
            if (!registeredHotkey) hook.KeyPressed += new EventHandler<KeyPressedEventArgs>(hook_KeyPressed);
            // register the control + alt + F12 combination as hot key.
            hook.RegisterHotKey(ModifierKeys.None, (Keys)settings.RecordingHotKey); //ModifierKeys.Control | ModifierKeys.Alt, Keys.F12
            registeredHotkey = true;
        }

        private void hook_KeyPressed(object sender, KeyPressedEventArgs e)
        {
            if (currentSettings.alsoTakeScreenshot) takeScreenshot();
            saveReplay();
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
                MainWindow.Instance.replayBufferStatus_Output.Text = "Recording...";
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
                this.Dispatcher.Invoke(() => MainWindow.Instance.replayBufferStatus_Output2.Text = "Saving Replay to " + mp4);
                Thread.Sleep(3000);
                this.Dispatcher.Invoke(() => MainWindow.Instance.replayBufferStatus_Output2.Text = "");
            });
        }
        public void takeScreenshot()
        {
            var bmpScreenshot = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            var gfxScreenshot = Graphics.FromImage(bmpScreenshot);
            gfxScreenshot.CopyFromScreen(Screen.PrimaryScreen.Bounds.X, Screen.PrimaryScreen.Bounds.Y, 0, 0, Screen.PrimaryScreen.Bounds.Size, CopyPixelOperation.SourceCopy);

            string savePath = null;
            if (!string.IsNullOrEmpty(viewModel.ScreenshotsPath) && Directory.Exists(viewModel.ScreenshotsPath)) savePath = viewModel.ScreenshotsPath;
            else
            {
                savePath = "./Screenshots";
                Directory.CreateDirectory(savePath);
            }

            string screenshotpath = $"{savePath}/Screenshot_{DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss")}.png";
            bmpScreenshot.Save(screenshotpath, System.Drawing.Imaging.ImageFormat.Png);
            Task.Run(() =>
            {
                this.Dispatcher.Invoke(() => MainWindow.Instance.autoRecordStatus_Output2.Text = "Saved Screenshot to " + screenshotpath);
                Thread.Sleep(3000);
                this.Dispatcher.Invoke(() => MainWindow.Instance.autoRecordStatus_Output2.Text = "");
            });
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            klutchfinding = false;
            viewModel.KlutchIdFinderLoading = Visibility.Collapsed;
        }
    }
}
