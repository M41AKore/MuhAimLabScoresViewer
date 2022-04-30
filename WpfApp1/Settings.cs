using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Xml.Serialization;

namespace MuhAimLabScoresViewer
{
    [XmlRoot]
    public class Settings
    {
        [XmlElement]
        public string SteamLibraryPath { get; set; }

        [XmlElement]
        public string klutchId { get; set; }

        [XmlElement]
        public string lastBenchmarkFile { get; set; }

        [XmlElement]
        public string lastCompetitionFile { get; set; }

        [XmlElement]
        public Key RecordingHotKey { get; set; }
    }
}
