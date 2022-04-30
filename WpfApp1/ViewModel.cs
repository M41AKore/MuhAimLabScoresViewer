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
        private Visibility borderVisible = Visibility.Collapsed;
        public Visibility BorderVisible
        {
            get => borderVisible;
            set {
                borderVisible = value;
                NotifyPropertyChanged("BorderVisible");
            }
        }

        private bool isRecording = false;
        public bool IsRecording
        {
            get => isRecording;
            set
            {
                isRecording = value;
                NotifyPropertyChanged("IsRecording");
            }
        }

        private string _ReplayBufferSeconds = "90";
        public string ReplayBufferSeconds
        {
            get => _ReplayBufferSeconds;
            set
            {
                _ReplayBufferSeconds = value;
                NotifyPropertyChanged("ReplayBufferSeconds");
            }
        }

        private void NotifyPropertyChanged(string info)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(info));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
