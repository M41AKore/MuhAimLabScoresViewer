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
