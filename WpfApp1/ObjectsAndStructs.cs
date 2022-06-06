using System;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MuhAimLabScoresViewer
{
    public class ObjectsAndStructs
    {
        public struct LeaderboardDisplay
        {
            public int place { get; set; }
            public string player { get; set; }
            public int score { get; set; }
            public int hit { get; set; }
            public int miss { get; set; }
            public string acc { get; set; }
        }
        public class LevelAndWeapon
        {
            public string taskname;
            public string level;
            public string weapon;
        }
        public class BenchmarkDisplay
        {
            public string task { get; set; }
            public string highscore { get; set; }
            public string mythic { get; set; }
            public string immortal { get; set; }
            public string archon { get; set; }
            public string ethereal { get; set; }
            public string divine { get; set; }
        }
        public class HighscoreUpdateCall
        {
            public string apicall { get; set; }
            public string taskname { get; set; }
            public string highscore { get; set; }
            public string parentTaskName { get; set; }
        }

        public class PartExpiraton
        {
            public Part part;
            public TimeSpan timeTillExpiration;
        }

        public class ScenarioHistory
        {
            public string Identification;
            public string Name;
            public List<Play> Plays;
            public string WorkshopId;
        }

        public class Play
        {
            public string DateString;
            public DateTime Date;
            public string Score;
            public string Accuracy;
        }

        public class Holder
        {
            public double X { get; set; }
            public double Y { get; set; }
            public Point Point { get; set; }

            public Holder()
            {
            }
        }

        public class Value
        {
            public double X { get; set; }
            public double Y { get; set; }

            public Value(double x, double y)
            {
                X = x;
                Y = y;
            }
        }

        public class Performance
        {
            public float hitsTotal { get; set; }     //":21.0,
            public float missesTotal { get; set; }  //"missesTotal":0.0,
            public float avgDist { get; set; }      //"avgDist":10.1594334,
            public float damageTotal { get; set; }  //"damageTotal":3.0,
            public double timePerKill { get; set; } //"timePerKill":0.283424944,
            public int killTotal { get; set; }      //"killTotal":21,
            public int targetsTotal { get; set; }   //"targetsTotal":24,
            public float accTotal { get; set; }     //"accTotal":100.0
        }
    }
}
