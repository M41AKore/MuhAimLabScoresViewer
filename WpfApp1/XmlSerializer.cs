using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MuhAimLabScoresViewer
{
    public static class XmlSerializer
    {
        public static T? deserializeXml<T>(string path)
        {
            try
            {
                System.Xml.Serialization.XmlSerializer reader = new System.Xml.Serialization.XmlSerializer(typeof(T));
                StreamReader file = new StreamReader(path);
                if(file != null)
                {                
                    var obj = reader.Deserialize(file);
                    if(obj != null)
                    {
                        file.Close();
                        return (T)obj;
                    }                   
                    else file.Close();
                }              
            }
            catch(Exception ex)
            {
                Logger.log("XmlSerializer Exception: " + Environment.NewLine + ex.Message);
            }

            return default; //default(T)
        }

        public static void serializeToXml<T>(T obj, string path)
        {
            var writer = new System.Xml.Serialization.XmlSerializer(typeof(T));
            var wfile = new StreamWriter(path);
            writer.Serialize(wfile, obj);
            wfile.Close();
        }
    }
}
