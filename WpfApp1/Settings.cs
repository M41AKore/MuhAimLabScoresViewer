using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using System.Xml.Serialization;
using static MuhAimLabScoresViewer.MainWindow;

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


        public static void loadSettings()
        {
            if (!File.Exists("./settings.xml"))
            {
                //File.WriteAllText("./settings.xml", "<Settings><SteamLibraryPath></SteamLibraryPath><klutchId></klutchId></Settings>");
                XmlSerializer.serializeToXml<Settings>(new Settings(), settingsPath);
            }

            var settings = XmlSerializer.deserializeXml<Settings>("./settings.xml");
            if (settings != null)
            {
                currentSettings = settings;
                MainWindow.Instance.SteamLibraryInput.Text = settings.SteamLibraryPath;
                MainWindow.Instance.klutchIdInput.Text = settings.klutchId;
                if (settings.RecordingHotKey != null) MainWindow.Instance.registerRecordingHotkey(settings);
                MainWindow.Instance.recordHotkeySet.Content = settings.RecordingHotKey.ToString();
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

                if (settings.lastBenchmarkFile != null && File.Exists(settings.lastBenchmarkFile)) MainWindow.Instance.HandleFile(settings.lastBenchmarkFile);
                if (settings.lastCompetitionFile != null && File.Exists(settings.lastCompetitionFile)) MainWindow.Instance.HandleFile(settings.lastCompetitionFile);
            }
        }
        public static void SaveSettings()
        {
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

            if (viewModel.LastBenchmarkPath != null && File.Exists(viewModel.LastBenchmarkPath)) currentSettings.lastBenchmarkFile = viewModel.LastBenchmarkPath;
            if (viewModel.LastCompetitionPath != null && File.Exists(viewModel.LastCompetitionPath)) currentSettings.lastCompetitionFile = viewModel.LastCompetitionPath;
            XmlSerializer.serializeToXml<Settings>(currentSettings, settingsPath);
        }        
    }
}
