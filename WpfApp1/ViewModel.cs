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
