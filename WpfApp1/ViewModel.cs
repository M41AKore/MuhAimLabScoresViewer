using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MuhAimLabScoresViewer
{
    public class ViewModel : INotifyPropertyChanged
    {
        private Visibility _borderVisible = Visibility.Collapsed;
        private bool _IsRecording = false;
        private string _ReplayBufferSeconds = "90s";
        private bool _onSaveReplayTakeScreenshot;
        private string _ScreenshotsPath;
        private string _ReplaysPath;
        private string _LastBenchmarkPath;
        private string _LastCompetitionPath;
        private string _OBS_Key;
        private bool _AutoRecord;
        private bool _AutoRecordDuplicates;
        private bool _ColorBenchmarkRanksAndScores;
        private int _LiveTrackerMinutes;
        private bool _LiveTrackerEnabled;

        public Visibility BorderVisible
        {
            get => _borderVisible;
            set {
                _borderVisible = value;
                NotifyPropertyChanged("BorderVisible");
            }
        }      
        public bool IsRecording
        {
            get => _IsRecording;
            set
            {
                _IsRecording = value;
                NotifyPropertyChanged("IsRecording");
            }
        }
        public string ReplayBufferSeconds
        {
            get => _ReplayBufferSeconds;
            set
            {
                _ReplayBufferSeconds = value;
                NotifyPropertyChanged("ReplayBufferSeconds");
            }
        }
        public bool onSaveReplayTakeScreenshot
        {
            get => _onSaveReplayTakeScreenshot;
            set
            {
                _onSaveReplayTakeScreenshot = value;
                NotifyPropertyChanged("onSaveReplayTakeScreenshot");
            }
        }
        public string ScreenshotsPath
        {
            get => _ScreenshotsPath;
            set
            {
                _ScreenshotsPath = value;
                NotifyPropertyChanged("ScreenshotsPath");
            }
        }
        public string ReplaysPath
        {
            get => _ReplaysPath;
            set
            {
                _ReplaysPath = value;
                NotifyPropertyChanged("ReplaysPath");
            }
        }
        private List<string> _ReplayBufferSecondsOptions;
        public List<string> ReplayBufferSecondsOptions
        {
            get => _ReplayBufferSecondsOptions;
            set
            {
                _ReplayBufferSecondsOptions = value;
                NotifyPropertyChanged("ReplayBufferSecondsOptions");
            }
        }
        public string LastBenchmarkPath
        {
            get => _LastBenchmarkPath;
            set
            {
                _LastBenchmarkPath = value;
                NotifyPropertyChanged("LastBenchmarkPath");
            }
        }
        public string LastCompetitionPath
        {
            get => _LastCompetitionPath; 
            set
            {
                _LastCompetitionPath = value;
                NotifyPropertyChanged("LastCompetitionPath");
            }
        }
        public string OBS_Key
        {
            get => _OBS_Key;
            set
            {
                _OBS_Key = value;
                NotifyPropertyChanged("OBS_Key");
            }
        }
        public bool AutoRecord
        {
            get => _AutoRecord;
            set
            {
                if (_AutoRecord != value)
                {
                    if (!_AutoRecord && value) LiveTracker.setupFileWatch();
                    else if(_AutoRecord && !value) LiveTracker.removeFileWatch();
                }
                _AutoRecord = value;
                NotifyPropertyChanged("AutoRecord");
            }
        }
        public  bool AutoRecordDuplicates
        {
            get => _AutoRecordDuplicates;
            set
            {
                _AutoRecordDuplicates = value;
                NotifyPropertyChanged("AutoRecordDuplicates");
            }
        }
        public bool ColorBenchmarkRanksAndScores
        {
            get => _ColorBenchmarkRanksAndScores;
            set
            {
                _ColorBenchmarkRanksAndScores = value;
                NotifyPropertyChanged("ColorBenchmarkRanksAndScores");
            }
        }
        public bool LiveTrackerEnabled
        {
            get => _LiveTrackerEnabled;
            set
            {
                _LiveTrackerEnabled = value;
                NotifyPropertyChanged("LiveTrackerEnabled");
            }
        }
        public int LiveTrackerMinutes
        {
            get => _LiveTrackerMinutes;
            set
            {
                _LiveTrackerMinutes = value;
                NotifyPropertyChanged("LiveTrackerMinutes");
            }
        }

        private void NotifyPropertyChanged(string info)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(info));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public ViewModel()
        {
            ReplayBufferSecondsOptions = new List<string>()
            {
                "60s",
                "90s",
                "120s",
            };
        }
    }
}
