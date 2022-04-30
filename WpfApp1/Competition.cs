using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

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
    
    
        
    }

    public class Part
    {
        public string Name { get; set; }   
        public string Startdate { get; set; }
        public string Enddate { get; set; }
        public Scenario[] Scenarios { get; set; }    
    }

    public class Scenario
    {
        public string Name { get; set; }
        //public string APICall { get; set; }
        //public string LaunchCommand { get; set; }

        [XmlIgnore]
        public CompetitionTaskLeaderboard leaderboard { get; set; }
    }
}
