using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;
using System.Xml.Serialization;
using static MuhAimLabScoresViewer.MainWindow;

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


        private static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }

        public static void buildCompContenders()
        {
            try
            {
                currentComp.competitionContenders = new List<CompetitionContender>();

                for (int i = 0; i < currentComp.Parts.Length; i++)
                    for (int j = 0; j < currentComp.Parts[i].Scenarios.Length; j++)
                        foreach (var player in currentComp.Parts[i].Scenarios[j].leaderboard.ResultsItem.results)
                        {
                            var playdate = UnixTimeStampToDateTime(double.Parse(player.endedAt.Substring(0, player.endedAt.Length - 3)));

                            // limit eligible scores to within time frame
                            //var playdate = DateTime.UnixEpoch.AddSeconds(long.Parse(player.endedAt.Substring(0, player.endedAt.Length - 3)));

                            /*if (player.klutchId == "A1C827457D314A65") //player.klutchId == "31A5D3DD1FF64BC9"
                            {
                                Logger.log($"for {currentComp.Parts[i].Scenarios[j].Name} and play of score '{player.score}'");
                                Logger.log("adjusting timestamp of '" + playdate.ToString() + "' to '" + (playdate.Hour >= 12 ? playdate.AddHours(12).ToString() : playdate.ToString()) + "'");
                            }*/

                            //if(playdate.Hour >= 12) playdate = playdate.AddHours(12); 

                            var compPartEnddate = DateTime.Parse(currentComp.Parts[i].Enddate);
                            var compStartDate = DateTime.Parse(currentComp.Parts[i].Startdate);

                            //if (playdate < compStartDate || playdate > compPartEnddate) continue;
                            if (playdate < compStartDate.AddHours(-12) || playdate > compPartEnddate.AddHours(12)) continue;

                            var existingPlayer = currentComp.competitionContenders.FirstOrDefault(c => c.klutchId == player.klutchId);
                            if (existingPlayer == null) //create new
                            {
                                existingPlayer = new CompetitionContender()
                                {
                                    Name = player.username,
                                    klutchId = player.klutchId,
                                    mostRecentTimestamp = long.Parse(player.endedAt), //1 649 388 232 000
                                };
                                currentComp.competitionContenders.Add(existingPlayer);
                            }

                            //try to use newest name
                            if (existingPlayer.Name != player.username)
                                if (long.TryParse(player.endedAt, out long playTimestamp) && playTimestamp > existingPlayer.mostRecentTimestamp)
                                {
                                    existingPlayer.Name = player.username;
                                    existingPlayer.mostRecentTimestamp = playTimestamp;
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
