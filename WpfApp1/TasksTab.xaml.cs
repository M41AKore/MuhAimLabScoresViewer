using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static MuhAimLabScoresViewer.APIStuff;
using static MuhAimLabScoresViewer.APIStuff.LeaderboardResult;

namespace MuhAimLabScoresViewer
{
    /// <summary>
    /// Interaction logic for TasksTab.xaml
    /// </summary>
    public partial class TasksTab : Page
    {
        public static ViewModel viewModel;
        LeaderboardResult currentTaskResults;
        string currentTaskResultsName;
        int currentTaskResultsLastPage = int.MaxValue;

        public TasksTab()
        {
            InitializeComponent();
            DataContext = MainWindow.viewModel;
            viewModel = MainWindow.viewModel;
        }
        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) getLeaderboardFor((sender as TextBox).Text);
        }

        private void taskPageButton_Next_Click(object sender, RoutedEventArgs e)
        {
            if (currentTaskResults != null)
            {
                viewModel.currentTaskPageIndex = currentTaskResultsLastPage + 1 > currentTaskResultsLastPage ? currentTaskResultsLastPage : viewModel.currentTaskPageIndex + 1;

                var stuff = Helper.getLevelAndWeaponForTask(currentTaskResultsName);
                APIStuff.runLeaderboardQuery(stuff.level.Replace("%20", " "), stuff.weapon, viewModel.currentTaskPageIndex * 100, (viewModel.currentTaskPageIndex + 1) * 100).ContinueWith(result =>
                {
                    if (result == null || result.Result == null)
                    {
                        viewModel.currentTaskPageIndex--;
                        currentTaskResultsLastPage = viewModel.currentTaskPageIndex;
                        this.Dispatcher.Invoke(() => taskPageButton_Next.Visibility = Visibility.Hidden); //must be on last page, hide next button
                    }
                    else
                        this.Dispatcher.Invoke(() =>
                        {
                            if (viewModel.currentTaskPageIndex + 1 >= currentTaskResultsLastPage) taskPageButton_Next.Visibility = Visibility.Hidden;
                            if (viewModel.currentTaskPageIndex > 0) taskPageButton_Previous.Visibility = Visibility.Visible;
                            addTaskLeaderboardData(result.Result.aimlab.leaderboard.data, leaderboardStacky);
                            currentTaskResults = result.Result;
                        });
                });
            }
        }
        
        private void taskPageButton_Previous_Click(object sender, RoutedEventArgs e)
        {
            if (currentTaskResults != null)
            {
                viewModel.currentTaskPageIndex = viewModel.currentTaskPageIndex - 1 < 0 ? 0 : viewModel.currentTaskPageIndex - 1;

                var stuff = Helper.getLevelAndWeaponForTask(currentTaskResultsName);
                APIStuff.runLeaderboardQuery(stuff.level.Replace("%20", " "), stuff.weapon, viewModel.currentTaskPageIndex * 100, (viewModel.currentTaskPageIndex + 1) * 100).ContinueWith(result =>
                {
                    if (result == null || result.Result == null)
                    {
                        viewModel.currentTaskPageIndex = 0;
                        this.Dispatcher.Invoke(() => taskPageButton_Previous.Visibility = Visibility.Hidden); //must be on first page, hide previous button
                    }
                    else
                        this.Dispatcher.Invoke(() =>
                        {
                            if (viewModel.currentTaskPageIndex <= 0) taskPageButton_Previous.Visibility = Visibility.Hidden;
                            if (viewModel.currentTaskPageIndex < currentTaskResultsLastPage) taskPageButton_Next.Visibility = Visibility.Visible;
                            addTaskLeaderboardData(result.Result.aimlab.leaderboard.data, leaderboardStacky);
                            currentTaskResults = result.Result;
                        });
                });
            }
        }

        private void getLeaderboardFor(string taskName)
        {
            if (!Directory.Exists(viewModel.SteamLibraryPath))
            {
                MainWindow.Instance.showMessageBox("please set SteamLibraryPath in Settings!");
                return;
            }

            var stuff = Helper.getLevelAndWeaponForTask(taskName);
            if (stuff != null) APIStuff.runLeaderboardQuery(stuff.level.Replace("%20", " "), stuff.weapon, 0, 100).ContinueWith(result => populateleaderboard(result.Result, taskName));
            else
                Task.Run(() =>
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        leaderboardStacky.Children.Clear();
                        searchInfo.Visibility = Visibility.Visible;
                    });
                    Thread.Sleep(3000);
                    this.Dispatcher.Invoke(() => searchInfo.Visibility = Visibility.Hidden);
                });
        }
        
        private void populateleaderboard(LeaderboardResult result, string taskName)
        {
            try
            {
                if (result != null)
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        addTaskLeaderboardHeaders(leaderboardheaderStacky);
                        addTaskLeaderboardData(result.aimlab.leaderboard.data, leaderboardStacky);
                        currentTaskResults = result;
                        currentTaskResultsName = taskName;
                        viewModel.currentTaskPageIndex = 0;
                        if (viewModel.currentTaskPageIndex <= 0) taskPageButton_Previous.Visibility = Visibility.Hidden;
                    });
                }
                else Logger.log($"{taskName} API call did not return a result!");
            }
            catch (Exception e)
            {
                MainWindow.Instance.showMessageBox(e.Message);
            }
        }
        
        private void addTaskLeaderboardHeaders(StackPanel parent)
        {
            parent.Children.Clear();
            var headerDocky = new DockPanel()
            {
                Margin = new Thickness(-15, 2, 5, 2),
                //Background = Brushes.Red
                Width = 380,
            };

            headerDocky.Children.Add(new TextBlock()
            {
                Text = "Place",
                Width = 40,
            });
            headerDocky.Children.Add(new TextBlock()
            {
                Text = "Player",
                Width = 120,
            });
            headerDocky.Children.Add(new TextBlock()
            {
                Text = "Score",
                Width = 60,
            });
            headerDocky.Children.Add(new TextBlock()
            {
                Text = "Hits",
                Width = 40,
            });
            headerDocky.Children.Add(new TextBlock()
            {
                Text = "Misses",
                Width = 40,
            });
            headerDocky.Children.Add(new TextBlock()
            {
                Text = "Accuracy",
                Width = 60,
            });

            if(viewModel.ShowUserTaskDuration)
            {
                headerDocky.Children.Add(new TextBlock()
                {
                    Text = "Duration",
                    Width = 60,
                });
                headerDocky.Width = 440;
            }
            
            parent.Children.Add(headerDocky);
        }
        
        private void addTaskLeaderboardData(List<leaderboardData> results, StackPanel parent)
        {
            leaderboardStacky.Children.Clear();

            for (int i = 0; i < results.Count; i++)
            {
                var entryDocky = new DockPanel()
                {
                    Margin = new Thickness(5, 2, 5, 2),
                    //Background = Brushes.Red
                    Width = 380
                };

                entryDocky.Children.Add(new TextBlock() { Width = 40, Text = $"#{results[i].rank}", }); // $"#{i + 1}", //placement

                entryDocky.Children.Add(new TextBlock() { Width = 120, Text = results[i].username, });  //player name

                entryDocky.Children.Add(new TextBlock() { Width = 60, Text = results[i].score.ToString(), }); //score

                entryDocky.Children.Add(new TextBlock() { Width = 40, Text = results[i].shots_hit.ToString(), }); //hits

                entryDocky.Children.Add(new TextBlock() { Width = 40, Text = results[i].shots_missed.ToString(), }); //misses

                entryDocky.Children.Add(new TextBlock() { Width = 60, Text = $"{results[i].accuracy.ToString("0.##")}%", }); //accuracy

                if (viewModel.ShowUserTaskDuration)
                {
                    entryDocky.Children.Add(new TextBlock() { Width = 60, Text = $"{results[i].task_duration.ToString("0.##")}s", }); //task duration
                    entryDocky.Width = 440;
                }

                leaderboardStacky.Children.Add(entryDocky);
            }
        }
    }
}
