using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MuhAimLabScoresViewer
{
    public class CompetitionTaskLeaderboard
    {
        public string TaskName { get; set; }
        public Item ResultsItem { get; set; }
        public double MeanScoreTop20 { get; set; }
        public double StandardDeviationTop20 { get; set; }

        public void calculateMeanTop20OfScenarios()
        {
            MeanScoreTop20 = (double)(ResultsItem.results.Take(20).Average(r => r.score));
            List<double> deviationList = new List<double>();
            ResultsItem.results.Take(20).ToList().ForEach(e => deviationList.Add(Math.Pow(e.score - MeanScoreTop20, 2)));
            StandardDeviationTop20 = Math.Sqrt(deviationList.Average());
        }
    }
}
