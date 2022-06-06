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
using static MuhAimLabScoresViewer.ObjectsAndStructs;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using GraphQL;

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

        public static async Task<HighscoreUpdateCall> getCompHighscore(HighscoreUpdateCall call)
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
                        if (r.klutchId == viewModel.klutchId)
                        {
                            call.highscore = r.score.ToString();
                            break;
                        }                          
                }
                catch (HttpRequestException e)
                {
                    Console.WriteLine("\nException Caught!");
                    Console.WriteLine("Message :{0} ", e.Message);
                }
            }

            return call;
        }

        public static async Task<HighscoreUpdateCall> getBenchHighscore(HighscoreUpdateCall call)
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

                    MainWindow.Instance.benchmarkLeaderboards.Add(new BenchmarkLeaderboard()
                    {
                        TaskName = call.taskname,
                        ResultsItem = item
                    });

                    foreach (var r in item.results)
                        if (r.klutchId == viewModel.klutchId)
                        {
                            call.highscore = r.score.ToString();
                            break;
                        }
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

        public static async Task<LeaderboardResult> runLeaderboardQuery(string taskName, string weaponName, int start, int end, string period = "all")
        {
            using var graphQLClient = new GraphQLHttpClient("https://api.aimlab.gg/graphql", new NewtonsoftJsonSerializer());
            var leaderboardRequest = new GraphQLRequest
            {
                Query = @"
                query getAimlabLeaderboard($leaderboardInput: LeaderboardInput!) {
                    aimlab {
                        leaderboard(input: $leaderboardInput) {
                            id
                            source
                            metadata {
                                offset
                                rows
                                totalRows
                                __typename
                            }
                            schema {
                                id
                                fields
                                __typename
                            }
                            data
                            __typename
                        }
                        __typename
                    }
                }",
                OperationName = "getAimlabLeaderboard",
            };

            if (period != "all")
            {
                leaderboardRequest.Variables = new
                {
                    leaderboardInput = new
                    {
                        clientId = "aimlab",
                        limit = end - start,
                        offset = start,
                        taskId = taskName,
                        taskMode = 0,
                        weaponId = weaponName
                    },
                    window = new
                    {
                        period = period,
                        value = DateTime.UtcNow.ToString("yyyy-MM-dd")
                    }
                };
            }
            else
            {
                leaderboardRequest.Variables = new
                {
                    leaderboardInput = new
                    {
                        clientId = "aimlab",
                        limit = end - start,
                        offset = start,
                        taskId = taskName,
                        taskMode = 0,
                        weaponId = weaponName
                    }
                };
            }

            try
            {
                var graphQLResponse = await graphQLClient.SendQueryAsync<LeaderboardResult>(leaderboardRequest);
                if (graphQLResponse != null) return graphQLResponse.Data;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return null;
        }

        public class LeaderboardResult
        {
            public AimLabContent aimlab { get; set; }
        }

        public class AimLabContent
        {
            public Leaderboard leaderboard { get; set; }
            public string __typename { get; set; }
        }

        public class Leaderboard
        {
            public string id { get; set; }
            public string source { get; set; }
            public MetadataContent metadata { get; set; }
            public schema schema { get; set; }
            public List<leaderboardData> data { get; set; }
            public string __typename { get; set; }
        }

        public class MetadataContent
        {
            public int offset { get; set; }
            public int rows { get; set; }
            public int totalRows { get; set; }
            public string __typename { get; set; }
        }

        public class schema
        {
            public string id { get; set; }
            public List<field> fields { get; set; }
            public string __typename { get; set; }
        }

        public class field
        {
            public string name { get; set; }
            public bool reverse { get; set; }
            public string format { get; set; }
            public int precision { get; set; }
            public string label { get; set; }
            public string description { get; set; }
        }

        public class leaderboardData
        {
            public int rank { get; set; }
            public string user_id { get; set; }
            public int score { get; set; }
            public string ended_at { get; set; }
            public string task_id { get; set; }
            public int task_mode { get; set; }
            public string client_id { get; set; }
            public string weapon_id { get; set; }
            public int map_id { get; set; }
            public int task_duration { get; set; }
            public int targets { get; set; }
            public int kills { get; set; }
            public int shots_fired { get; set; }
            public int shots_hit { get; set; }
            public int shots_missed { get; set; }
            public double reaction_time { get; set; }
            public List<double?> reaction_time_segmets { get; set; }
            public float accuracy { get; set; }
            public List<int?> accuracy_segments { get; set; }
            public double shots_per_kill { get; set; }
            public float kills_per_sec { get; set; }
            public custom custom { get; set; }
            public string play_id { get; set; }
            public string username { get; set; }
        }

        public class custom
        {

        }

    }
}
