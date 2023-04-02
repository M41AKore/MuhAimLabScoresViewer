using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;
using System.Xml.Serialization;
using static MuhAimLabScoresViewer.MainWindow;
using static MuhAimLabScoresViewer.CompetitionTab;
using static MuhAimLabScoresViewer.Helper;
using System.Diagnostics;

namespace MuhAimLabScoresViewer
{
    [XmlRoot("Competition")]
    public class Competition
    {
        [XmlElement("Title")]
        public string Title { get; set; }

        [XmlArray("Parts")]
        [XmlArrayItem("Part")]
        public Part[] Parts { get; set; }

        [XmlIgnore]
        public List<CompetitionContender> competitionContenders { get; set; }

        public static void buildCompContenders()
        {
            try
            {
                currentComp.competitionContenders = new List<CompetitionContender>();

                for (int i = 0; i < currentComp.Parts.Length; i++)
                    for (int j = 0; j < currentComp.Parts[i].Scenarios.Length; j++)
                        foreach (var player in currentComp.Parts[i].Scenarios[j].leaderboard.Leaderboard.data)
                        {
                            var playdate = DateTime.Parse(player.ended_at);
                            var compPartEnddate = DateTime.Parse(currentComp.Parts[i].Enddate);
                            var compStartDate = DateTime.Parse(currentComp.Parts[i].Startdate);

                            //if (playdate < compStartDate || playdate > compPartEnddate) continue;
                            if (playdate < compStartDate.AddHours(-12) || playdate > compPartEnddate.AddHours(12)) continue; // <--- CURRENT TIMEFRAME LIMITATIONS

                            var existingPlayer = currentComp.competitionContenders.FirstOrDefault(c => c.klutchId == player.user_id);
                            if (existingPlayer == null) //create new
                            {
                                existingPlayer = new CompetitionContender()
                                {
                                    Name = player.username,
                                    klutchId = player.user_id,
                                    mostRecentTimestamp = playdate, //now is datetime in string format //1 649 388 232 000 <- REST
                                };
                                currentComp.competitionContenders.Add(existingPlayer);
                            }

                            //try to use newest name
                            if (existingPlayer.Name != player.username)
                                if (playdate > existingPlayer.mostRecentTimestamp)
                                {
                                    existingPlayer.Name = player.username;
                                    existingPlayer.mostRecentTimestamp = playdate;
                                }

                            if (existingPlayer.partResults == null)
                            {
                                existingPlayer.partResults = new CompetitorCompetitionPart[currentComp.Parts.Length];
                                for (int k = 0; k < currentComp.Parts.Length; k++)
                                    existingPlayer.partResults[k] = new CompetitorCompetitionPart()
                                    {
                                        taskResults = new CompetitionTaskResult[currentComp.Parts[k].Scenarios.Length]
                                    };
                            }

                            existingPlayer.partResults[i].taskResults[j] = new CompetitionTaskResult()
                            {
                                taskname = currentComp.Parts[i].Scenarios[j].TaskName,
                                score = player.score
                            };
                        }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            //calculate points
            var allLeaderboards = new List<CompetitionTaskLeaderboard>();
            foreach (var part in currentComp.Parts)
                foreach (var scen in part.Scenarios)
                {
                    scen.leaderboard.calculateMeanTop20OfScenarios();
                    allLeaderboards.Add(scen.leaderboard);
                }

            currentComp.competitionContenders.ForEach(c => c.calculateScorePoints(allLeaderboards)); //points results from top20 score deviation
            currentComp.competitionContenders = currentComp.competitionContenders.OrderByDescending(c => c.totalPoints).ToList();
        }
    }

    [XmlRoot("Part")]
    public class Part
    {
        [XmlElement("Name")]
        public string Name { get; set; }

        [XmlElement("Startdate")]
        public string Startdate { get; set; }

        [XmlElement("Enddate")]
        public string Enddate { get; set; }

        [XmlArray("Scenarios")]
        [XmlArrayItem("Scenario")]
        public Scenario[] Scenarios { get; set; }
    }

    [XmlRoot("Scenario")]
    public class Scenario
    {
        [XmlElement("TaskName")]
        public string TaskName { get; set; }

        [XmlElement("DisplayName")]
        public string DisplayName { get; set; }
        
        //public string LaunchCommand { get; set; }

        [XmlIgnore]
        public CompetitionTaskLeaderboard leaderboard { get; set; }
    }
}
