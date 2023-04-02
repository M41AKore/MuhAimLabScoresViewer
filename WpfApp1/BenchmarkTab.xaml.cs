using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static MuhAimLabScoresViewer.APIStuff;
using static MuhAimLabScoresViewer.Helper;

namespace MuhAimLabScoresViewer
{
    /// <summary>
    /// Interaction logic for BenchmarkTab.xaml
    /// </summary>
    public partial class BenchmarkTab : Page
    {
        private ViewModel viewModel;
        Stopwatch timer = new Stopwatch();
        public static Benchmark currentBenchmark;

        public BenchmarkTab()
        {
            InitializeComponent();
            DataContext = MainWindow.viewModel;
            viewModel = MainWindow.viewModel;
        }
        public void NameTB_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && e.ClickCount == 2)
            {
                var tb = sender as TextBlock;
                var part = tb.Name.Substring(11, tb.Name.Length - 11);
                string link = generateLink(part); // $"playtask_{parts[0]}_{parts[1]}",   0 = authorid, 1 = workshopid

                ProcessStartInfo processStartInfo = new ProcessStartInfo(link);
                processStartInfo.UseShellExecute = true;
                System.Diagnostics.Process.Start(processStartInfo);
            }
        }
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            benchStacky.Children.Clear();
            if (viewModel.LastBenchmarkPath != null) HandleFile(viewModel.LastBenchmarkPath);
        }
        private void DragDropInput(object sender, DragEventArgs e) => getFileDrop(e);
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

            if (!string.IsNullOrEmpty(viewModel.SteamLibraryPath) && Directory.Exists(viewModel.SteamLibraryPath))
            {
                if (filename.ToLower().Contains("benchmark") || filename.ToLower().Contains("bench"))
                {
                    try
                    {
                        var newbench = XmlSerializer.deserializeXml<Benchmark>(filepath);
                        if (newbench != null)
                        {
                            currentBenchmark = newbench;
                            viewModel.LastBenchmarkPath = filepath;
                            Benchmark.addBenchmarkGUIHeaders(benchStacky);
                            Benchmark.addBenchmarkScores(benchStacky);
                            loadBenchmarkToGUI(newbench);
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.log("Error during benchmark loading!" + Environment.NewLine + e.Message.ToString());
                        currentBenchmark = null;
                        viewModel.LastBenchmarkPath = null;
                        MainWindow.Instance.showMessageBox("Could not load this Benchmark file!");
                    }
                }
            }
            else MainWindow.Instance.showMessageBox("please enter 'SteamLibraryPath' in Settings!");
        }

        //benchmark scores
        private async Task loadBenchmarkToGUI(Benchmark bench)
        {
            Trace.WriteLine("time taken for reading file = " + timer.ElapsedMilliseconds);
            timer.Restart();

            if (!string.IsNullOrEmpty(viewModel.klutchId))
            {
                var r = await APIStuff.runGetProfileAGG("", viewModel.klutchId);
                foreach (var playedTask in r.aimlab.plays_agg)
                    foreach (var category in bench.Categories)
                        foreach (var subcategory in category.Subcategories)
                            foreach (var scenario in subcategory.Scenarios)
                                if (scenario.Name == playedTask.group_by.task_name
                                    || scenario.AlternativeName == playedTask.group_by.task_name
                                    || playedTask.group_by.task_id == "CsLevel.rA hebe.rA Three.R2IW6T" && scenario.Name == "rA Threewide Small"
                                    || scenario.Name == playedTask.group_by.task_id || scenario.AlternativeName == playedTask.group_by.task_id)
                                {
                                    if (int.TryParse(playedTask.aggregate.max.score, out int score) && scenario.Score < score)
                                        scenario.Score = score;
                                        
                                    updateBenchmarkWithHighscore(score, scenario.Name);
                                }

                Txt_BenchmarkRank.Text = Benchmark.calculateBenchmarkRank(benchStacky);
                //Txt_BenchmarkRank.Foreground = getColorFromHex(currentBenchmark.Ranks.FirstOrDefault(r => r.Name == rankName).Color);
                Txt_BenchmarkEnergy.Text = ((int)currentBenchmark.TotalEnergy).ToString();
                //createBenchmarkLeaderboard(currentBenchmark);
            }
            else MainWindow.Instance.showMessageBox("please set 'klutchId' in Settings!");
        }
        private string calculateUIScoreItemName(string taskName)
        {
            for (int i = 0; i < currentBenchmark.Categories.Length; i++)
                for (int j = 0; j < currentBenchmark.Categories[i].Subcategories.Length; j++)
                    for (int k = 0; k < currentBenchmark.Categories[i].Subcategories[j].Scenarios.Length; k++)
                        if (currentBenchmark.Categories[i].Subcategories[j].Scenarios[k].Name == taskName) return $"score_{i}_{j}_{k}";

            return null;
        }
        private void updateBenchmarkWithHighscore(int score, string taskName)
        {
            timer.Restart();
            Trace.WriteLine("updating '" + taskName + "'...");

            var targetName = calculateUIScoreItemName(taskName);
            if (targetName != null)
            {
                this.Dispatcher.Invoke(() =>
                {
                    TextBlock tb = Benchmark.benchScoreFieldLookup.FirstOrDefault(f => f.Key == targetName).Value;
                    if (tb != null && int.TryParse(tb.Text, out int parentTaskScore) && parentTaskScore < score) tb.Text = score.ToString();
                });
            }
            Trace.WriteLine($"updated '{taskName}' in {timer.ElapsedMilliseconds} ms!");
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SheetsInteraction.Init();
            SheetsInteraction.updateSheetWithBenchmark(currentBenchmark);
        }
    }
}
