using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Serialization;
using static MuhAimLabScoresViewer.MainWindow;
using static MuhAimLabScoresViewer.SettingsTab;

namespace MuhAimLabScoresViewer
{
    [XmlRoot]
    public class Settings
    {
        [XmlElement]
        public string SteamLibraryPath { get; set; }

        [XmlElement]
        public string klutchId { get; set; }

        [XmlElement]
        public string lastBenchmarkFile { get; set; }

        [XmlElement]
        public string lastCompetitionFile { get; set; }

        [XmlElement]
        public Keys? RecordingHotKey { get; set; }

        [XmlElement]
        public bool alsoTakeScreenshot { get; set; }

        [XmlElement]
        public string ReplaySavePath { get; set; }

        [XmlElement]
        public string ScreenshotSavePath { get; set; }

        [XmlElement]
        public string BufferSeconds { get; set; }

        [XmlElement]
        public string OBS_Hotkey { get; set; }

        [XmlElement]
        public bool AutoRecord { get; set; }

        [XmlElement]
        public bool AutoRecordDuplicates { get; set; }

        [XmlElement]
        public bool ColorBenchmarkRanksAndScores { get; set; }

        [XmlElement]
        public int LiveTrackerMinutes { get; set; }

        [XmlElement]
        public bool LiveTrackerEnabled { get; set; }

        [XmlElement]
        public string BenchmarkSheetId { get; set; }

        [XmlElement]
        public bool ShowUserTaskDuration { get; set; }

        [XmlElement]
        public string OBSoutputDirectory { get; set; }

        [XmlElement]
        public int VODrenameDelay { get; set; } //seconds


        public static void loadSettings()
        {
            try
            {
                if (!File.Exists("./settings.xml")) XmlSerializer.serializeToXml<Settings>(new Settings(), settingsPath);

                var settings = XmlSerializer.deserializeXml<Settings>("./settings.xml");
                if (settings != null)
                {
                    viewModel.klutchId = settings.klutchId;
                    var tab = MainWindow.Instance.windowTabs.First(t => t.Title == "Settings") as SettingsTab;
                    if (settings.RecordingHotKey != null) tab.registerRecordingHotkey(settings);
                    tab.recordHotkeySet.Content = settings.RecordingHotKey?.ToString();
                    viewModel.onSaveReplayTakeScreenshot = settings.alsoTakeScreenshot;
                    viewModel.ScreenshotsPath = settings.ScreenshotSavePath;
                    viewModel.ReplaysPath = settings.ReplaySavePath;
                    viewModel.ReplayBufferSeconds = settings.BufferSeconds;
                    viewModel.OBS_Key = settings.OBS_Hotkey;
                    viewModel.AutoRecord = settings.AutoRecord;
                    viewModel.AutoRecordDuplicates = settings.AutoRecordDuplicates;
                    viewModel.ColorBenchmarkRanksAndScores = settings.ColorBenchmarkRanksAndScores;
                    viewModel.LiveTrackerEnabled = settings.LiveTrackerEnabled;
                    viewModel.LiveTrackerMinutes = settings.LiveTrackerMinutes;
                    viewModel.BenchmarkSpreadSheetId = settings.BenchmarkSheetId;
                    viewModel.ShowUserTaskDuration = settings.ShowUserTaskDuration;
                   
                    viewModel.ScreenshotsPath = settings.ScreenshotSavePath;
                    viewModel.OBSoutputDirectory = settings.OBSoutputDirectory;
                    viewModel.VODrenameDelay = settings.VODrenameDelay;

                    currentSettings = settings;

                    if (settings.SteamLibraryPath != null && Directory.Exists(settings.SteamLibraryPath))
                        viewModel.SteamLibraryPath = settings.SteamLibraryPath;

                    if (settings.lastBenchmarkFile != null && File.Exists(settings.lastBenchmarkFile))
                        (MainWindow.Instance.windowTabs[1] as BenchmarkTab).HandleFile(settings.lastBenchmarkFile);

                    if (settings.lastCompetitionFile != null && File.Exists(settings.lastCompetitionFile))
                        (MainWindow.Instance.windowTabs[1] as CompetitionTab).HandleFile(settings.lastCompetitionFile);
                }
            }
            catch (Exception ex)
            {
                MainWindow.Instance.showMessageBox(ex.Message);
            }
        }

        public static void SaveSettings()
        {
            try
            {
                if (currentSettings != null)
                {
                    if (Directory.Exists(viewModel.SteamLibraryPath) || string.IsNullOrEmpty(viewModel.SteamLibraryPath)) currentSettings.SteamLibraryPath = viewModel.SteamLibraryPath;
                    if (File.Exists(viewModel.LastBenchmarkPath) || string.IsNullOrEmpty(viewModel.LastBenchmarkPath)) currentSettings.lastBenchmarkFile = viewModel.LastBenchmarkPath;
                    if (File.Exists(viewModel.LastCompetitionPath) || string.IsNullOrEmpty(viewModel.LastCompetitionPath)) currentSettings.lastCompetitionFile = viewModel.LastCompetitionPath;

                    currentSettings.klutchId = viewModel.klutchId;
                    currentSettings.alsoTakeScreenshot = viewModel.onSaveReplayTakeScreenshot;
                    currentSettings.ScreenshotSavePath = viewModel.ScreenshotsPath;
                    currentSettings.ReplaySavePath = viewModel.ReplaysPath;
                    currentSettings.BufferSeconds = viewModel.ReplayBufferSeconds;
                    currentSettings.OBS_Hotkey = viewModel.OBS_Key;
                    currentSettings.AutoRecord = viewModel.AutoRecord;
                    currentSettings.AutoRecordDuplicates = viewModel.AutoRecordDuplicates;
                    currentSettings.ColorBenchmarkRanksAndScores = viewModel.ColorBenchmarkRanksAndScores;
                    currentSettings.LiveTrackerEnabled = viewModel.LiveTrackerEnabled;
                    currentSettings.LiveTrackerMinutes = viewModel.LiveTrackerMinutes;
                    currentSettings.BenchmarkSheetId = viewModel.BenchmarkSpreadSheetId;
                    currentSettings.ShowUserTaskDuration = viewModel.ShowUserTaskDuration;

                    currentSettings.ScreenshotSavePath = viewModel.ScreenshotsPath;
                    currentSettings.OBSoutputDirectory = viewModel.OBSoutputDirectory;
                    currentSettings.VODrenameDelay = viewModel.VODrenameDelay;

                    XmlSerializer.serializeToXml<Settings>(currentSettings, settingsPath);
                }
            }
            catch (Exception ex)
            {
                MainWindow.Instance.showMessageBox(ex.Message);
            }
        }        
    }
}
