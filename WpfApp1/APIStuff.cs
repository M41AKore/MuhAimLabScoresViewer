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
    public static class APIStuff
    {
        /** 
         * Please unify these methods into a single generic one... tried but parently me succ
         * */

        public static async Task<Item> httpstuff(string call)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    //f.e. "https://apiclient.aimlab.gg/leaderboards/scores?taskSlug=CsLevel.rA%20hebe.rA%20x%20Aim.R9GSEI&weaponName=Custom_rA100hz&map=42&mode=42&timeWindow=all");
                    HttpResponseMessage response = await client.GetAsync(call);
                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();
                    var item = JsonConvert.DeserializeObject<Item>(responseBody);
                    return item;
                }
                catch (HttpRequestException e)
                {
                    Console.WriteLine("\nException Caught!");
                    Console.WriteLine("Message :{0} ", e.Message);
                }
            }
            return null;
        }

        public static async Task<HighscoreUpdateCall> getHighscore(HighscoreUpdateCall call)
        {
            if (string.IsNullOrEmpty(call.apicall)) return call;

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = await client.GetAsync(call.apicall);
                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();
                    Item item = JsonConvert.DeserializeObject<Item>(responseBody);

                    foreach (var r in item.results)
                        if (r.klutchId == currentSettings.klutchId)
                            call.highscore = r.score.ToString();
                }
                catch (HttpRequestException e)
                {
                    Console.WriteLine("\nException Caught!");
                    Console.WriteLine("Message :{0} ", e.Message);
                }
            }

            return call;
        }

        public static async Task<Item> getCompTaskLeaderboard(string apicall)
        {
            Item item = null;
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = await client.GetAsync(apicall);
                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();
                    item = JsonConvert.DeserializeObject<Item>(responseBody);
                }
                catch (HttpRequestException e)
                {
                    Console.WriteLine("\nException Caught!");
                    Console.WriteLine("Message :{0} ", e.Message);
                }
            }
            return item;
        }
    }
}
