using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MuhAimLabScoresViewer
{
    public class Item
    {
        public Schema schema { get; set; }
        public Result[] results { get; set; }
        public bool cached { get; set; }
        public string source { get; set; }
    }

    public class Schema
    {
        public SchemaColumn[] columns { get; set; }
    }

    public class SchemaColumn
    {
        public string name { get; set; }
        public string displayName { get; set; }
        public int direction { get; set; }
        public string format { get; set; }
    }

    public class Result
    {
        public int score { get; set; }
        public string acctotal { get; set; }
        public int hitstotal { get; set; }
        public int missestotal { get; set; }
        public int targetstotal { get; set; }
        public string avgdist { get; set; }
        public string username { get; set; }
        public string klutchId { get; set; }
        public string playId { get; set; }
        public string endedAt { get; set; }
    }
}
