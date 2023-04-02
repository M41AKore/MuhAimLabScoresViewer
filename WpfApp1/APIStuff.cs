using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using GraphQL;
using System.Collections.Concurrent;
using static MuhAimLabScoresViewer.APIStuff.LeaderboardResult;
using System.Threading;

namespace MuhAimLabScoresViewer
{
    public static class APIStuff
    {
        private static GraphQLHttpClient graphQLClient = null;
        private static int COUNTER;

        public static async Task<LeaderboardResult> runLeaderboardQuery(string taskName, string weaponName, int start, int end, string period = "all")
        {
            if (graphQLClient == null) graphQLClient = new GraphQLHttpClient("https://api.aimlab.gg/graphql", new NewtonsoftJsonSerializer());
            //using var graphQLClient = new GraphQLHttpClient("https://api.aimlab.gg/graphql", new NewtonsoftJsonSerializer());
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
                Interlocked.Increment(ref COUNTER);
                if (graphQLResponse != null) return graphQLResponse.Data;
                //Console.WriteLine("raw response:");
                //Console.WriteLine(JsonSerializer.Serialize(graphQLResponse, new JsonSerializerOptions { WriteIndented = true }));
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
                public float task_duration { get; set; }
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

        private static async Task<int> runAPIcall(string taskId, string weaponName)
        {
            Leaderboard? fullboard = null;
            int minScore = 1;
            int start = 0;
            int end = 100;
            int maxResults = int.MaxValue;
            var concResults = new ConcurrentBag<List<leaderboardData>>();

            var firstCall = await runLeaderboardQuery(taskId, weaponName, 0, 100);
            if (firstCall != null)
            {
                fullboard = firstCall.aimlab.leaderboard;
                maxResults = firstCall.aimlab.leaderboard.metadata.totalRows;
                if (maxResults < 100 || fullboard.data.Count < 100 || fullboard.data.Last().score < minScore) 
                {
                    //got all
                }
                else
                {
                    var lookUp = new ConcurrentDictionary<int, int>();
                    var tasks = new List<Task>();
                    while (end < 500 && end < maxResults) //get the rest, if necessary
                    {
                        start = end;
                        end += 100;
                        lookUp.TryAdd(start, end);
                        tasks.Add(Task.Run(() => some(lookUp, taskId, weaponName, concResults)));
                    }
                    await Task.WhenAll(tasks);

                    foreach (var list in concResults.OrderByDescending(b => b[0].score))
                        fullboard.data.AddRange(list);
                }
            }
     
            if(fullboard != null)
                foreach (var user in fullboard.data)
                    if (user.user_id == "") return user.score;
            
            return 0;
        }
        private static async Task<List<leaderboardData>> some(ConcurrentDictionary<int, int> lookUp, string taskId, string weaponName, ConcurrentBag<List<leaderboardData>> bag)
        {
            var item = lookUp.First();
            lookUp.TryRemove(item);
            var r = await runLeaderboardQuery(taskId, weaponName, item.Key, item.Value);
            bag.Add(r.aimlab.leaderboard.data);
            return r.aimlab.leaderboard.data;
        }

        public class AGGProfileResult
        {
            public AimlabProfile aimlabProfile { get; set; }
            public class AimlabProfile
            {
                public Ranking ranking { get; set; }
                public SkillScore[] skillScores { get; set; }

                public class Ranking
                {
                    public Rank rank;
                    public string skill { get; set; }
                    public string __typename { get; set; }
                    public class Rank
                    {
                        public string displayName { get; set; }
                        public string tier { get; set; }
                        public string level { get; set; }
                        public string minSkill { get; set; }
                        public string maxSkill { get; set; }
                        public string __typename { get; set; }
                    }
                }
                public class SkillScore
                {
                    public string name { get; set; }
                    public string score { get; set; }
                    public string __typename { get; set; }
                }

                public string __typename { get; set; }
            }

            public AimLab aimlab { get; set; }
            public class AimLab
            {
                public PlayAGG[] plays_agg { get; set; }

                public class PlayAGG
                {
                    public GroupBy group_by { get; set; }
                    public Aggregate aggregate { get; set; }
                    public string __typename { get; set; }

                    public class GroupBy
                    {
                        public string task_id { get; set; }
                        public string task_name { get; set; }
                        public string task_mode_mod { get; set; }
                        public string skill_id { get; set; }
                        public string __typename { get; set; }
                    }
                    public class Aggregate
                    {
                        public string count { get; set; }

                        public Avg avg { get; set; }
                        public class Avg
                        {
                            public string score { get; set; }
                            public string accuracy { get; set; }
                            public string reaction_time { get; set; }
                            public string shots_per_kill { get; set; }
                            public string kills_per_sec { get; set; }
                            public string __typename { get; set; }
                        }

                        public Min min { get; set; }
                        public class Min
                        {
                            public string reaction_time { get; set; }
                            public string __typename { get; set; }
                        }

                        public Max max { get; set; }
                        public class Max
                        {
                            public string created_at { get; set; }
                            public string score { get; set; }
                            public string accuracy { get; set; }
                            public string shots_per_kill { get; set; }
                            public string kills_per_sec { get; set; }
                            public string __typename { get; set; }
                        }

                        public string __typename { get; set; }
                    }
                }

                public string __typename { get; set; }
            }
        }
        public static async Task<AGGProfileResult> runGetProfileAGG(string user, string klutchid)
        {
            using var graphQLClient = new GraphQLHttpClient("https://api.aimlab.gg/graphql", new NewtonsoftJsonSerializer());
            var leaderboardRequest = new GraphQLRequest
            {
                Query = @"query GetAimlabProfileAgg($username: String, $where: AimlabPlayWhere!) {
                aimlabProfile(username: $username) {
                  ranking {
                    rank {
                      displayName
                      tier
                      level
                      minSkill
                      maxSkill
                      __typename
                    }
                    skill
                    __typename
                  }
                  skillScores {
                    name
                    score
                    __typename
                  }
                  __typename
                }
                aimlab {
                  plays_agg(where: $where) {
                    group_by {
                      task_id
                      task_name
                      task_mode_mod
                      skill_id
                      __typename
                    }
                    aggregate {
                      count
                      avg {
                        score
                        accuracy
                        reaction_time
                        shots_per_kill
                        kills_per_sec
                        __typename
                      }
                      min {
                        reaction_time
                        __typename
                      }
                      max {
                        created_at
                        score
                        accuracy
                        shots_per_kill
                        kills_per_sec
                        __typename
                      }
                      __typename
                    }
                    __typename
                  }
                  __typename
                }
               }",
                OperationName = "GetAimlabProfileAgg",
                Variables = new
                {
                    username = user,
                    where = new
                    {
                        user_id = new
                        {
                            _eq = klutchid
                        },
                        is_practice = new
                        {
                            _eq = false
                        },
                        score = new
                        {
                            _gt = 0
                        }
                    }
                }
            };

            try
            {
                var graphQLResponse = await graphQLClient.SendQueryAsync<AGGProfileResult>(leaderboardRequest);
                if (graphQLResponse != null)
                {
                    //Console.WriteLine("raw response:");
                    //Console.WriteLine(JsonSerializer.Serialize(graphQLResponse, new JsonSerializerOptions { WriteIndented = true }));
                    return graphQLResponse.Data;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return null;
        }
    }
}
