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

        [XmlArray("Categories")]
        [XmlArrayItem("Category")]
        public Category[] Categories { get; set; }

        public class RankEnergy
        {
            public string TaskName;
            public int Score;
            public string RankTitle;
        }

        public static string calculateBenchmarkRank(StackPanel benchStacky)
        {
            for (int i = 0; i < currentBenchmark.Categories.Length; i++)
            {
                for (int j = 0; j < currentBenchmark.Categories[i].Subcategories.Length; j++)
                {
                    for (int k = 0; k < currentBenchmark.Categories[i].Subcategories[j].Scenarios.Length; k++)
                    {
                        int score = int.Parse(findBenchmarkScoreFieldWithName($"score_{i}_{j}_{k}", benchStacky).Text);
                        string rank = currentBenchmark.Categories[i].Subcategories[j].Scenarios[k].RankRequirements.FirstOrDefault(rr => int.Parse(rr.RankScore) <= score).RankTitle;

                        var energy = new RankEnergy()
                        {
                            TaskName = currentBenchmark.Categories[i].Subcategories[j].Scenarios[k].Name,
                            Score = score,
                            RankTitle = rank
                        };
                        
                    }
                }
            }

            return "divi- jk, noob";
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
            var someScen = currentBenchmark.Categories.First(c => c.Subcategories.Any()).Subcategories.First(s => s.Scenarios.Any()).Scenarios.FirstOrDefault();
            if (someScen != null)
            {
                for (int i = 0; i < someScen.RankRequirements.Length; i++)
                {
                    ranksDocky.Children.Add(new TextBlock()
                    {
                        Name = $"rankheader_{someScen.RankRequirements[i].RankTitle}",
                        Text = someScen.RankRequirements[i].RankTitle,
                        Width = 100,
                        TextAlignment = TextAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        FontWeight = FontWeights.Bold,
                    });
                }
            }
            headerdocky.Children.Add(ranksDocky);

            benchStacky.Children.Add(new Border()
            {
                Background = Brushes.LightBlue,
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
                        foreach(var requirement in task.RankRequirements)
                        {
                            requirementsDocky.Children.Add(new TextBlock()
                            {
                                Name = $"requirement_{i}_{j}_{k}_{requirement.RankTitle}",
                                Text = requirement.RankScore,
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

        [XmlArray("RankRequirements")]
        [XmlArrayItem("RankRequirement")]
        public RankRequirement[] RankRequirements { get; set; }
    }

    [XmlRoot("RankRequirement")]
    public class RankRequirement
    {
        [XmlElement("RankTitle")]
        public string RankTitle { get; set; }

        [XmlElement("RankScore")]
        public string RankScore { get; set; }
    }
}
