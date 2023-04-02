using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MuhAimLabScoresViewer
{
    /// <summary>
    /// Interaction logic for AimLabHistoryTab.xaml
    /// </summary>
    public partial class AimLabHistoryTab : Page
    {
        Stopwatch timer = new Stopwatch();

        public AimLabHistoryTab()
        {
            InitializeComponent();
            DataContext = MainWindow.viewModel;
        }

        public void DragDropInput(object sender, DragEventArgs e) => getFileDrop(e);
        private void getFileDrop(DragEventArgs e)
        {
            if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop))
                foreach (string file in (string[])e.Data.GetData(System.Windows.DataFormats.FileDrop))
                    HandleFile(file);
        }
        public void HandleFile(string filepath)
        {
            timer.Start();
            string filename = filepath.Split('\\').Last();

            if (!string.IsNullOrEmpty(MainWindow.viewModel.SteamLibraryPath) && Directory.Exists(MainWindow.viewModel.SteamLibraryPath))
            {
                if (filename.ToLower().Contains("taskData") || filename.ToLower().Contains(".json"))
                {
                    try
                    {
                        if (File.Exists(filepath))
                        {
                            var item = AimLabHistoryViewer.getData(filepath);
                            if (item != null) AimLabHistoryViewer.sortTasks(item.historyEntries);
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.log("Error during taskdata loading!" + Environment.NewLine + e.Message.ToString());
                        MainWindow.Instance.showMessageBox("Could not load this TaskData file!");
                    }
                }
            }
            else MainWindow.Instance.showMessageBox("please enter 'SteamLibraryPath' in Settings!");
        }

        private void Button_Click_4(object sender, RoutedEventArgs e) => AimLabHistoryViewer.pullDataFromLocalDB();
    }
}
