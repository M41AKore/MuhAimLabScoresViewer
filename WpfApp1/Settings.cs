using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
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
        public Keys? RecordingHotKey { get; set; }

        [XmlElement]
        public bool alsoTakeScreenshot { get; set; }

        [XmlElement]
        public string ReplaySavePath { get; set; }

        [XmlElement]
        public string ScreenshotSavePath { get; set; }
    }
}
