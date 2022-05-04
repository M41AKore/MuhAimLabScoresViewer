﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MuhAimLabScoresViewer
{
    public class AimLabHistory
    {
        public HistoryEntry[] historyEntries;
    }

    public class HistoryEntry
    {
        public int taskId { get; set; } // 1, 2, 3
        public string klutch_id { get; set; } //"A1C827457D314A65",
        public string create_date { get; set; } //2021-11-20T12:55:49",
        public string taskName { get; set; }  //": "CsLevel8b02d36a-3178-41b5-88c6-dd8d7574b695",
        public string score { get; set; }  //": 749,
        public string mode { get; set; }  //": 42,
        public string aimlab_map { get; set; }  //": 42,
        public string aimlab_version { get; set; }  //": "v0.95.6.3-1992",
        public string weaponType { get; set; }  //": "Heavy",
        public string weaponName { get; set; }  //": "LG",
        public string performanceClass { get; set; }  //": "CSTask",
        public string workshopId { get; set; }  //": null,
        public string performance { get; set; }  //{\"hitsTotal\":749.0,\"missesTotal\":449.0,\"avgDist\":21.547823,\"damageTotal\":1.0,\"timePerKill\":0.0,\"killTotal\":0,\"targetsTotal\":1,\"accTotal\":62.5208664}"
    }

    public class Performance
    {
        public string hitsTotal { get; set; }
        public string missesTotal { get; set; }
        public string avgDist { get; set; }
        public string damageTotal { get; set; }
        public string timePerKill { get; set; }
        public string killTotal { get; set; }
        public string targetsTotal { get; set; }
        public string accTotal { get; set; }
    }
}