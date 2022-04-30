using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using static MuhAimLabScoresViewer.MainWindow;
using System.Windows;

namespace MuhAimLabScoresViewer
{
    public static class APIHandler
    {
        /**public static async Task<Item> httpstuff(string call)
        {     
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    //f.e. "https://apiclient.aimlab.gg/leaderboards/scores?taskSlug=CsLevel.rA%20hebe.rA%20x%20Aim.R9GSEI&weaponName=Custom_rA100hz&map=42&mode=42&timeWindow=all");
                    HttpResponseMessage response = await client.GetAsync(call);
                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<Item>(responseBody);
                }
                catch (HttpRequestException e)
                {
                    Console.WriteLine("\nException Caught!");
                    Console.WriteLine("Message :{0} ", e.Message);
                }
            }
            return null;
        }*/
        
    }
}
