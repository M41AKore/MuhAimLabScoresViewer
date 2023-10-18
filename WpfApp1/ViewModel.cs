using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MuhAimLabScoresViewer
{
    public class ViewModel : INotifyPropertyChanged
    {
        public class ScenarioSortingType
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
        public class ScenarioSortingDirection
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        private string _SteamLibraryPath;
        private string _klutchId;
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
        private ObservableCollection<ScenarioSortingType> _SortTypes;
        private ObservableCollection<ScenarioSortingDirection> _SortDirections;
        private ScenarioSortingType _SortType;
        private ScenarioSortingDirection _SortDirection;
        private int _currentTaskPageIndex = 0;
        private string _BenchmarkSpreadSheetId;
        private Visibility _KlutchIdFinderLoading = Visibility.Collapsed;
        private List<string> _GraphPlayDisplayCounts;
        private string _GraphPlayDisplayCount;
        private bool _showTaskDuration;
        private string _HighscoreVODname;
        private string _OBSoutputDirectory;
        private int _VODrenameDelay;

        public string SteamLibraryPath
        {
            get => _SteamLibraryPath;
            set
            {
                _SteamLibraryPath = value;
                NotifyPropertyChanged("SteamLibraryPath");
            }
        }
        public string klutchId
        {
            get => _klutchId;
            set
            {
                _klutchId = value;
                NotifyPropertyChanged("klutchId");
            }
        }
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
                    else if (_AutoRecord && !value) LiveTracker.removeFileWatch();
                }
                _AutoRecord = value;
                NotifyPropertyChanged("AutoRecord");
            }
        }
        public bool AutoRecordDuplicates
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
        public ObservableCollection<ScenarioSortingType> SortTypes
        {
            get => _SortTypes;
            set
            {
                _SortTypes = value;
                NotifyPropertyChanged("SortTypes");
            }
        }
        public ObservableCollection<ScenarioSortingDirection> SortDirections
        {
            get => _SortDirections;
            set
            {
                _SortDirections = value;
                NotifyPropertyChanged("SortDirections");
            }
        }
        public ScenarioSortingType SortType
        {
            get => _SortType;
            set
            {
                if (_SortType != value && _SortType != null && SortDirection != null && AimLabHistoryViewer.Scenarios != null)
                {
                    AimLabHistoryViewer.createScenariosGUI(value.Name, SortDirection.Name);
                }
                _SortType = value;
                NotifyPropertyChanged("SortType");
            }
        }
        public ScenarioSortingDirection SortDirection
        {
            get => _SortDirection;
            set
            {
                if (_SortDirection != value && _SortDirection != null && SortType != null && AimLabHistoryViewer.Scenarios != null)
                {
                    AimLabHistoryViewer.createScenariosGUI(SortType.Name, value.Name);
                }
                _SortDirection = value;
                NotifyPropertyChanged("SortDirection");
            }
        }
        public int currentTaskPageIndex
        {
            get => _currentTaskPageIndex;
            set {
                _currentTaskPageIndex = value;
                NotifyPropertyChanged("currentTaskPageIndex");
            }
        }
        public string BenchmarkSpreadSheetId
        {
            get => _BenchmarkSpreadSheetId;
            set
            {
                _BenchmarkSpreadSheetId = value;
                NotifyPropertyChanged("BenchmarkSpreadSheetId");
            }
        }
        public Visibility KlutchIdFinderLoading
        {
            get => _KlutchIdFinderLoading;
            set
            {
                _KlutchIdFinderLoading = value;
                NotifyPropertyChanged("KlutchIdFinderLoading");
            }
        }
        public List<string> GraphPlayDisplayCounts
        {
            get => _GraphPlayDisplayCounts;
            set {
                _GraphPlayDisplayCounts = value;
                NotifyPropertyChanged("GraphPlayDisplayCounts");
            }
        }
        public string GraphPlayDisplayCount
        {
            get => _GraphPlayDisplayCount;
            set
            {
                if (_GraphPlayDisplayCount != value && _GraphPlayDisplayCount != null)
                {
                    AimLabHistoryViewer.createDataPoints(AimLabHistoryViewer.currentScenario, value);
                }
                _GraphPlayDisplayCount = value;
                NotifyPropertyChanged("GraphPlayDisplayCount");
            }
        }
        public bool ShowUserTaskDuration
        {
            get => _showTaskDuration;
            set
            {
                _showTaskDuration = value;
                NotifyPropertyChanged("ShowUserTaskDuration");
            }
        }
        public string HighscoreVODname
        {
            get => _HighscoreVODname;
            set
            {
                _HighscoreVODname = value;
                NotifyPropertyChanged("HighscoreVODname");
            }
        }
        public string OBSoutputDirectory
        {
            get => _OBSoutputDirectory;
            set
            {
                _OBSoutputDirectory = value;
                NotifyPropertyChanged("OBSoutputDirectory");
            }
        }
        public int VODrenameDelay
        {
            get => _VODrenameDelay;
            set
            {
                _VODrenameDelay = value;
                NotifyPropertyChanged("VODrenameDelay");
            }
        }



        private void NotifyPropertyChanged(string info)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(info));
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public ViewModel()
        {
            ReplayBufferSecondsOptions = new List<string>()
            {
                "60s",
                "90s",
                "120s",
            };

            SortTypes = new ObservableCollection<ScenarioSortingType>()
            {
                new ScenarioSortingType(){ Id=0, Name="Name"},
                new ScenarioSortingType(){ Id=1, Name="Plays"},
                new ScenarioSortingType(){ Id=2, Name="Date"},
            };
            SortType = SortTypes[0];

            SortDirections = new ObservableCollection<ScenarioSortingDirection>()
            {
                new ScenarioSortingDirection(){ Id=0, Name="Ascending"},
                new ScenarioSortingDirection(){ Id=1, Name="Descending"},
            };
            SortDirection = SortDirections[0];

            GraphPlayDisplayCounts = new List<string>
            {
                "all",
                "last 50",
                "last 100",
                "last 200",
                "last 500",
                "last 1000",
            };
            GraphPlayDisplayCount = GraphPlayDisplayCounts[0];
        }
    }
}
