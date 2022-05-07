using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using static MuhAimLabScoresViewer.MainWindow;

namespace MuhAimLabScoresViewer
{
    public class CompetitionContender
    {
        public string Name;
        public string klutchId;
        public long mostRecentTimestamp;
        public CompetitorCompetitionPart[] partResults;
        public double totalPoints;

        public void calculateScorePoints(List<CompetitionTaskLeaderboard> compLeaderboards)
        {
            for (int i = 0; i < currentComp.Parts.Length; i++)
            {
                if(partResults[i] == null)
                {
                    partResults[i] = new CompetitorCompetitionPart()
                    {
                        partPoints = 0,
                        taskResults = new CompetitionTaskResult[currentComp.Parts[i].Scenarios.Length]
                    };
                }

                for (int j = 0; j < currentComp.Parts[i].Scenarios.Length; j++)
                {
                    if (partResults[i].taskResults[j] == null)
                    {
                        partResults[i].taskResults[j] = new CompetitionTaskResult() 
                        {
                            taskname = currentComp.Parts[i].Scenarios[j].TaskName,
                            score = 0,
                            points = 0
                        };
                    }
                    var leaderboard = compLeaderboards.FirstOrDefault(l => l.TaskName == partResults[i].taskResults[j].taskname);
                    if (leaderboard != null)
                    {
                        var p = 100 + 30 * ((partResults[i].taskResults[j].score - leaderboard.MeanScoreTop20) / leaderboard.StandardDeviationTop20);
                        partResults[i].taskResults[j].points = p < 0 ? 0 : p;
                    }                     
                }
                partResults[i].calculatePartPoints();
            }
            totalPoints = partResults.Sum(p => p.partPoints);
        }
    }
    public class CompetitorCompetitionPart
    {
        public double partPoints;
        public CompetitionTaskResult[] taskResults;

        public void calculatePartPoints()
        {
            var p = taskResults.Sum(r => r.points) / taskResults.Length;
            partPoints = p < 0 ? 0 : p;
        }
    }
    public class CompetitionTaskResult
    {
        public string taskname;
        public int score;
        public double points;
    }
}
