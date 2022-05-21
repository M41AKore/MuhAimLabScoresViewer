using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media;
using static MuhAimLabScoresViewer.MainWindow;
using static MuhAimLabScoresViewer.ObjectsAndStructs;

namespace MuhAimLabScoresViewer
{
    public static class Helper
    {
        public static string uglyCleanup(string s)
        {
            s = s.Substring(s.IndexOf(':'), s.Length - s.IndexOf(':'));
            s = s.Trim(new char[] { ':', '\\', '"' });
            s = s.Replace("\\r", string.Empty);
            s = s.Trim('"');
            s = s.Replace('"', ' ');
            s = s.Trim();
            //seems to do it...
            return s;
        }
        public static SolidColorBrush getColorFromHex(string hexaColor)
        {
            return new SolidColorBrush(Color.FromArgb(255,
                    Convert.ToByte(hexaColor.Substring(1, 2), 16),
                    Convert.ToByte(hexaColor.Substring(3, 2), 16),
                    Convert.ToByte(hexaColor.Substring(5, 2), 16)));
        }
        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }

        public static string buildAPICallFromTaskName(string task)
        {
            if (!Directory.Exists(viewModel.SteamLibraryPath)) return null;

            DirectoryInfo[] dirs = new DirectoryInfo(viewModel.SteamLibraryPath + @"\steamapps\workshop\content\714010").GetDirectories();
            foreach (var dir in dirs)
                foreach (var subdir in dir.GetDirectories())
                    if (subdir.Name == "Levels")
                        foreach (var file in subdir.GetDirectories()[0].GetFiles())
                            if (file.Name == "level.es3")
                            {
                                var content = File.ReadAllText(file.FullName);
                                if (content.Contains(task))
                                {
                                    var levelandweapon = collectLevelAndWeaponFromES3(content, out string foundTaskName);
                                    if(foundTaskName == task)
                                        return "https://apiclient.aimlab.gg/leaderboards/scores?taskSlug=" +
                                                                                levelandweapon.level + "&weaponName=" + levelandweapon.weapon + "&map=42&mode=42&timeWindow=all";                                   
                                }
                            }

            return null;
        }
        private static LevelAndWeapon collectLevelAndWeaponFromES3(string filecontent, out string foundTaskName)
        {
            /** didn't work out cause stupid escape characters and \t\r\t\t spam
             * 
            //Regex regex = new Regex("contentMetadata.+?(?=\"id)"); //"contentMetadata.+?(?=\"category)"

                                        var matches = regex.Matches(content);
                                        if(matches.Count > 0)
                                        {
                                            var str = matches[0].Value.ToString();
                                            //"contentMetadata" : {
				                                //   "id" : "CsLevel.rA hebe.f96a4d2c.R2GOSC",
				                                //    "label" : "rA Twoshot",

                                            Regex r2 = new Regex("d\" : \".+\"");
                                            //   d" : "CsLevel.rA hebe.f96a4d2c.R2GOSC"
                                            var ms = r2.Matches(str);
                                            if(ms.Count > 0)
                                            {
                                                var taskname = ms[0].Value.ToString().Substring(6, ms[0].Value.ToString().Length - 2);
                                                Console.WriteLine("found task '" + taskname + "'");
                                            }
                                        }
                                        */

            var start = filecontent.IndexOf("contentMetadata");
            var relevant = filecontent.Substring(start, filecontent.IndexOf("Skybox") - start);

            var lines = relevant.Split(new string[] { "\",", "{", "}" }, StringSplitOptions.RemoveEmptyEntries);

            var semirelevantlines = lines.Where(l => l.Contains("id\"") || l.Contains("label\"") || l.Contains("Weapon\"")).ToList();

            var idline = semirelevantlines.Where(l => l.Contains("id\"")).FirstOrDefault();
            var labelline = semirelevantlines.Where((l) => l.Contains("label\"")).FirstOrDefault();
            var weaponline = semirelevantlines.FirstOrDefault(l => l.Contains("Weapon\""));

            idline = uglyCleanup(idline);
            labelline = uglyCleanup(labelline);
            foundTaskName = labelline;
            weaponline = uglyCleanup(weaponline);

            var result = new LevelAndWeapon()
            {
                taskname = labelline,
                level = idline.Replace(" ", "%20"),
                weapon = weaponline
            };

            return result;
        }

        public static string getTaskNameFromLevelID(string levelid, string workshopid, bool exclusive = true)
        {
            if (!Directory.Exists(viewModel.SteamLibraryPath)) return null;

            DirectoryInfo[] dirs = new DirectoryInfo(viewModel.SteamLibraryPath + @"\steamapps\workshop\content\714010").GetDirectories();
            foreach (var dir in dirs)
            {
                if (!string.IsNullOrEmpty(workshopid) && dir.Name != workshopid) continue;

                foreach (var subdir in dir.GetDirectories())
                    if (subdir.Name == "Levels")
                        foreach (var file in subdir.GetDirectories()[0].GetFiles())
                            if (file.Name == "level.es3")
                            {
                                var content = File.ReadAllText(file.FullName);
                                if (content.Contains(levelid))
                                {
                                    string foundLevelId = getLevelIDFromES3(content);
                                    if(foundLevelId == levelid || !exclusive) //with disabled "Yuki Aim - Gridshot"(label) counts as "gridshot"(id)
                                    {
                                        return getTaskNameFromES3(content);
                                    }                                
                                }
                            }
            }

            return null;
        }

        private static string getLevelIDFromES3(string filecontent)
        {
            var start = filecontent.IndexOf("contentMetadata");
            var relevant = filecontent.Substring(start, filecontent.IndexOf("category") - start);
            var lines = relevant.Split(new string[] { "\",", "{", "}" }, StringSplitOptions.RemoveEmptyEntries);
            var relevantline = lines.FirstOrDefault(l => l.Contains("id"));
            var result = uglyCleanup(relevantline);
            return result;
        }

        private static string getTaskNameFromES3(string filecontent)
        {
            var start = filecontent.IndexOf("contentMetadata");
            var relevant = filecontent.Substring(start, filecontent.IndexOf("category") - start);
            var lines = relevant.Split(new string[] { "\",", "{", "}" }, StringSplitOptions.RemoveEmptyEntries);
            var relevantline = lines.FirstOrDefault(l => l.Contains("label"));
            var result = uglyCleanup(relevantline);
            return result;
        }

        public static string buildAPICallFromTaskID(string weirdtaskid)
        {
            if (!Directory.Exists(viewModel.SteamLibraryPath)) return null;

            DirectoryInfo[] dirs = new DirectoryInfo(viewModel.SteamLibraryPath + @"\steamapps\workshop\content\714010").GetDirectories();
            foreach (var dir in dirs)
                foreach (var subdir in dir.GetDirectories())
                    if (subdir.Name == "Levels")
                        foreach (var file in subdir.GetDirectories()[0].GetFiles())
                            if (file.Name == "level.es3")
                            {
                                var content = File.ReadAllText(file.FullName);
                                if (content.Contains(weirdtaskid))
                                {
                                    var weapon = collectWeaponFromES3(content);
                                    return "https://apiclient.aimlab.gg/leaderboards/scores?taskSlug=" +
                                        weirdtaskid + "&weaponName=" + weapon + "&map=42&mode=42&timeWindow=all";
                                }
                            }

            return null;
        }
        public static string collectWeaponFromES3(string filecontent)
        {
            var start = filecontent.IndexOf("contentMetadata");
            var relevant = filecontent.Substring(start, filecontent.IndexOf("Skybox") - start);
            var lines = relevant.Split(new string[] { "\",", "{", "}" }, StringSplitOptions.RemoveEmptyEntries);
            var semirelevantlines = lines.Where(l => l.Contains("Weapon\"")).ToList();
            var weaponline = semirelevantlines.FirstOrDefault(l => l.Contains("Weapon\""));
            weaponline = uglyCleanup(weaponline);
            return weaponline;
        }

        public static string generateLink(string taskDisplayName)
        {
            var parts = taskDisplayName.Split('_');
            string workshopId = parts[1]; // "2765722547";
            string authorId = parts[0];  //"16BAE1433DACC70D";
            string link = "https://go.aimlab.gg/v1/redirects?link=aimlab://workshop?id=" + workshopId + "&source=" + authorId + "&link=steam://rungameid/714010";
            return link;
        }
        public static string getAuthorIdAndWorkshopIdFromTaskName(string taskname)
        {
            if (!Directory.Exists(viewModel.SteamLibraryPath)) return null;

            string result = null;

            DirectoryInfo[] dirs = new DirectoryInfo(viewModel.SteamLibraryPath + @"\steamapps\workshop\content\714010").GetDirectories();
            foreach (var dir in dirs)
                foreach (var subdir in dir.GetDirectories())
                    if (subdir.Name == "Levels")
                        foreach (var file in subdir.GetDirectories()[0].GetFiles())
                            if (file.Name == "level.es3")
                            {
                                var content = File.ReadAllText(file.FullName);
                                if (content.Contains(taskname))
                                {
                                    string pattern = "contentMetadata.*\\n.*\\n.*\\n.*\\n.*\\n.*\\n.*\\n.*\\n.*\\n.*\\n.*\\n\\s.*authorId.*\\n.*\\n.*,";
                                    var match = Regex.Match(content, pattern);
                                    if (match.Success)
                                    {
                                        var lines = match.Value.Split(Environment.NewLine);
                                        var authorIdParts = lines.FirstOrDefault(l => l.Contains("authorId")).Split('"');
                                        string author = authorIdParts[3];

                                        var workshopIdParts = lines.FirstOrDefault(l => l.Contains("publishSteamId")).Split(':');
                                        string cleanId = workshopIdParts[1].Replace('"', ' ').Replace(',', ' ').Trim();
                                        if (cleanId == "0" || cleanId == "null") cleanId = dir.Name;
                                        result = $"{author} {cleanId}";
                                    }
                                    return result;
                                }
                            }

            return result;
        }
    }
}
