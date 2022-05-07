using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml;
using System.Xml.Serialization;
using static MuhAimLabScoresViewer.MainWindow;

namespace MuhAimLabScoresViewer
{
    [XmlRoot("Benchmark")]
    public class Benchmark
    {
        [XmlElement("Title")]
        public string Title { get; set; }

        [XmlArray("Ranks")]
        [XmlArrayItem("Rank")]
        public Rank[] Ranks { get; set; }

        [XmlArray("Categories")]
        [XmlArrayItem("Category")]
        public Category[] Categories { get; set; }

        public List<float> EnergyPerTask { get; set; }
        public float TotalEnergy { get; set; }

        public static string calculateBenchmarkRank(StackPanel benchStacky)
        {
            currentBenchmark.EnergyPerTask = new List<float>();

            // collect scores and calculate energy
            for (int i = 0; i < currentBenchmark.Categories.Length; i++)
            {
                for (int j = 0; j < currentBenchmark.Categories[i].Subcategories.Length; j++)
                {
                    List<float> countingEnergy = new List<float>();

                    for (int k = 0; k < currentBenchmark.Categories[i].Subcategories[j].Scenarios.Length; k++)
                    {
                        int score = int.Parse(findBenchmarkScoreFieldWithName($"score_{i}_{j}_{k}", benchStacky).Text);
                        currentBenchmark.Categories[i].Subcategories[j].Scenarios[k].calculateEnergy(score);
                        countingEnergy.Add(currentBenchmark.Categories[i].Subcategories[j].Scenarios[k].Energy);
                    }

                    countingEnergy.Remove(countingEnergy.Min()); //best two scenarios count towards rank                  
                    currentBenchmark.EnergyPerTask.AddRange(countingEnergy);
                }
            }

            //get final rank
            currentBenchmark.TotalEnergy = currentBenchmark.EnergyPerTask.Sum();
            string? rankTitle = currentBenchmark.Ranks.LastOrDefault(r => r.RankEnergyRequirement < currentBenchmark.TotalEnergy)?.Name;

            return rankTitle != null ? rankTitle : "unranked";
        }
        public static void addBenchmarkGUIHeaders(StackPanel benchStacky)
        {
            benchStacky.Children.Add(new TextBlock()
            {
                Name = "header_benchmark",
                Text = currentBenchmark.Title,
                Margin = new Thickness(1),
                FontWeight = FontWeights.Bold,
                FontSize = 18
            });

            //headers (on top)
            var headerdocky = new DockPanel()
            {
                Name = "benchmark_headers",
                Width = 800,
                HorizontalAlignment = HorizontalAlignment.Left,
                Background = Brushes.LightGray,
                Margin = new Thickness(6,0,0,0), //to roughly match the offset created by borders of category and subcategory
            };
            //scenario
            headerdocky.Children.Add(new TextBlock()
            {
                Name = "header_scenario",
                Text = "Scenario",
                Width = 100,
                HorizontalAlignment = HorizontalAlignment.Center,
                FontWeight = FontWeights.Bold,
            });
            //player score
            headerdocky.Children.Add(new TextBlock()
            {
                Name = "header_score",
                Text = "Score",
                Width = 100,
                HorizontalAlignment = HorizontalAlignment.Center,
                TextAlignment = TextAlignment.Center,
                FontWeight = FontWeights.Bold,
            });
            //ranks
            var ranksDocky = new DockPanel() { HorizontalAlignment = HorizontalAlignment.Left };
            for (int i = 0; i < currentBenchmark.Ranks.Length; i++)
            {
                ranksDocky.Children.Add(new TextBlock()
                {
                    Name = $"rankheader_{currentBenchmark.Ranks[i].Name}",
                    Text = currentBenchmark.Ranks[i].Name,
                    Foreground = getColorFromHex(currentBenchmark.Ranks[i].Color),
                    Width = 100,
                    TextAlignment = TextAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    FontWeight = FontWeights.Bold,
                });
            }
            headerdocky.Children.Add(ranksDocky);

            benchStacky.Children.Add(new Border()
            {
                Background = Brushes.LightGray,
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(5),
                Padding = new Thickness(2),
                Child = headerdocky
            });
        }
        public static void addBenchmarkScores(StackPanel benchStacky)
        {
            for (int i = 0; i < currentBenchmark.Categories.Length; i++)
            {
                var categoryStacky = new StackPanel();

                //category name
                categoryStacky.Children.Add(new TextBlock()
                {
                    Name = $"category_{i}",
                    Text = currentBenchmark.Categories[i].Name,
                    Width = 100,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    TextAlignment = TextAlignment.Left
                });

                for (int j = 0; j < currentBenchmark.Categories[i].Subcategories.Length; j++)
                {
                    //subcategory name
                    var subcategoryStacky = new StackPanel();

                    subcategoryStacky.Children.Add(new TextBlock()
                    {
                        Name = $"subcategory_{i}_{j}",
                        Text = currentBenchmark.Categories[i].Subcategories[j].Name,
                        Width = 100,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        TextAlignment = TextAlignment.Left,
                    });

                    for (int k = 0; k < currentBenchmark.Categories[i].Subcategories[j].Scenarios.Length; k++)
                    {
                        var scenarioDocky = new DockPanel();
                        var task = currentBenchmark.Categories[i].Subcategories[j].Scenarios[k]; //shorthand
                    
                        //task name
                        scenarioDocky.Children.Add(new TextBlock()
                        {
                            Name = $"scenario_{i}_{j}_{k}",
                            Text = task.Name,
                            Width = 100,
                            HorizontalAlignment = HorizontalAlignment.Left,
                            TextAlignment= TextAlignment.Left,
                        });

                        //task score
                        scenarioDocky.Children.Add(new TextBlock()
                        {
                            Name = $"score_{i}_{j}_{k}",
                            Text = "0",
                            Width = 100,
                            HorizontalAlignment = HorizontalAlignment.Left,
                            TextAlignment = TextAlignment.Center
                        });

                        //rank requirements
                        var requirementsDocky = new DockPanel();
                        for (int r = 0; r < task.RankScoreRequirements.Length; r++)
                        {
                            requirementsDocky.Children.Add(new TextBlock()
                            {
                                Name = $"requirement_{i}_{j}_{k}_{r}",
                                Text = task.RankScoreRequirements[r].ToString(),
                                Width = 100,
                                HorizontalAlignment = HorizontalAlignment.Left,
                                TextAlignment = TextAlignment.Center
                            });
                        }
                        scenarioDocky.Children.Add(requirementsDocky);
                        subcategoryStacky.Children.Add(new Border()
                        {
                            Background = getColorFromHex("#eeeeee"),
                            BorderBrush = Brushes.Black,
                            BorderThickness = new Thickness(1),
                            CornerRadius = new CornerRadius(3),
                            Padding = new Thickness(2),
                            Child = scenarioDocky
                        });
                    }

                    categoryStacky.Children.Add(new Border()
                    {
                        Background = getColorFromHex("#dddddd"),
                        BorderBrush = Brushes.Black,
                        BorderThickness = new Thickness(1),
                        CornerRadius = new CornerRadius(3),
                        Padding = new Thickness(2),
                        Child = subcategoryStacky
                    });
                }

                benchStacky.Children.Add(new Border()
                {
                    Background = getColorFromHex("#cccccc"),
                    BorderBrush = Brushes.Black,
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(3),
                    Padding = new Thickness(2),
                    Child = categoryStacky
                });
            }
        }
        public static TextBlock findBenchmarkScoreFieldWithName(string targetName, StackPanel benchStacky)
        {
            TextBlock result = null;

            foreach (var c1 in benchStacky.Children)
            {
                if (c1 is Border)
                {
                    var category = (c1 as Border).Child as StackPanel;
                    if (category == null) continue;

                    foreach (var c2 in category.Children)
                    {
                        if (c2 is Border)
                        {
                            var subcategory = (c2 as Border).Child as StackPanel;
                            if (subcategory == null) continue;

                            foreach (var c3 in subcategory.Children)
                            {
                                if (c3 is Border)
                                {
                                    var scenario = (c3 as Border).Child as DockPanel;
                                    if (scenario == null) continue;

                                    foreach (var c4 in scenario.Children)
                                    {
                                        var field = c4 as TextBlock;
                                        if (field == null) continue;

                                        if (field.Name == targetName)
                                        {
                                            return field;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return result;
        }
    }

    [XmlRoot("Rank")]
    public class Rank
    {
        [XmlElement("Name")]
        public string Name { get; set; }

        [XmlElement("Color")]
        public string Color { get; set; }

        [XmlElement("TaskEnergyRequirement")]
        public int TaskEnergyRequirement { get; set; }

        [XmlElement("RankEnergyRequirement")]
        public int RankEnergyRequirement { get; set; }
    }

    [XmlRoot("Category")]
    public class Category
    {
        [XmlElement("Name")]
        public string Name { get; set; }

        [XmlArray("Subcategories")]
        [XmlArrayItem("Subcategory")]
        public Subcategory[] Subcategories { get; set; }
    }

    [XmlRoot("Subcategory")]
    public class Subcategory
    {
        [XmlElement("Name")]
        public string Name { get; set; }

        [XmlArray("Scenarios")]
        [XmlArrayItem("Scenario")]
        public BenchScenario[] Scenarios { get; set; }
    }

    [XmlRoot("Scenario")]
    public class BenchScenario
    {
        [XmlElement("Name")]
        public string Name { get; set; }

        [XmlElement("AlternativeName")]
        public string AlternativeName { get; set; }

        [XmlArray("RankScoreRequirements")]
        [XmlArrayItem("RankScore")]
        public int[] RankScoreRequirements { get; set; }


        public int Energy { get; set; }

        public void calculateEnergy(int score)
        {
            try
            {
                int rankindex = getAchievedRankScoreIndex(score); // returns -1 if unranked (Energy for that is TODO)
                if (rankindex >= 0)
                {
                    int energy = (int)getEnergyForScore(score, (int)rankindex);
                    Energy = energy;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);    
            }
        }

        private int getAchievedRankScoreIndex(int score)
        {
            for (int i = 0; i < RankScoreRequirements.Length; i++)
                if (RankScoreRequirements[i] > score) return i - 1;

            return -1;
        }

        private float getEnergyForScore(int score, int rankindex)
        {
            float baseRankEnergy = currentBenchmark.Ranks[rankindex].TaskEnergyRequirement;

            //score between two requirements
            int maxScoreDifference = !(rankindex < RankScoreRequirements.Length) ? //if already top rank
                RankScoreRequirements[rankindex] - RankScoreRequirements[rankindex-1] : //use energy per points of previous
                RankScoreRequirements[rankindex + 1] - RankScoreRequirements[rankindex]; 

            int maxEnergyDifference = !(rankindex < RankScoreRequirements.Length) ? //if already top rank
                currentBenchmark.Ranks[rankindex].TaskEnergyRequirement - currentBenchmark.Ranks[rankindex-1].TaskEnergyRequirement : //use energy per points of previous
                currentBenchmark.Ranks[rankindex + 1].TaskEnergyRequirement - currentBenchmark.Ranks[rankindex].TaskEnergyRequirement;

            float energyPerPointsRatio = (float)maxEnergyDifference / maxScoreDifference;

            int scoreDifference = score - RankScoreRequirements[rankindex]; //the player's score above the rank requirement score

            return baseRankEnergy + (scoreDifference * energyPerPointsRatio);
        }
    }
}
