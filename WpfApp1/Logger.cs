using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows;

namespace MuhAimLabScoresViewer
{
    public static class Logger
    {
        public static string logOutputPath = "./Logs/";
        public static string logOutputFile;

        public static void setup()
        {
            Directory.CreateDirectory(logOutputPath);

            string date = DateTime.Now.ToString("yyyy-MM-dd_hh-mm-ss");
            string newlogFile = Path.Combine(logOutputPath, $"Log_{date}.txt");

            try
            {
                File.WriteAllText(newlogFile, $"---- MuhAimLabScoresViewer Log - {date} ----" + Environment.NewLine);
                if (File.Exists(newlogFile)) logOutputFile = newlogFile;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }         
        }

        public static void log(string s)
        {
            try
            {
                File.AppendAllText(logOutputFile, s + Environment.NewLine);
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }          
        }
    }
}
