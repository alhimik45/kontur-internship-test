using System;
using System.Collections.Generic;
using System.Linq;
using Kontur.GameStats.Server.Data;
using Nancy.Bootstrapper;
using Nancy.Testing;

namespace Kontur.GameStats.Tests
{
    public class NancyTest
    {
        protected Browser Browser;
        protected INancyBootstrapper Bootstrapper;

        protected const string ServersInfoPath = "/servers/info";
        protected readonly Func<int?, string> RecentMatchesPath = count => "/reports/recent-matches" + (count.HasValue ? $"/{count.Value}" : "");
        protected readonly Func<int?, string> BestPlayersPath = count => "/reports/best-players" + (count.HasValue ? $"/{count.Value}" : "");
        protected readonly Func<int?, string> PopularServersPath = count => "/reports/popular-servers" + (count.HasValue ? $"/{count.Value}" : "");
        protected readonly Func<string, string> ServerStatsPath = endpoint => $"/servers/{endpoint}/stats";
        protected readonly Func<string, string> PlayerStatsPath = name => $"/players/{name}/stats";
        protected readonly Func<string, string> AdvertisePath = endpoint => $"/servers/{endpoint}/info";
        protected readonly Func<string, string, string> MatchInfoPath = (endpoint, timestamp) => $"/servers/{endpoint}/matches/{timestamp}";

        protected BrowserResponse Get(string path)
        {
            var result = Browser.Get(path, with =>
            {
                with.HttpRequest();
            });
            return result;
        }

        protected BrowserResponse SendAdvertise(string endpoint, AdvertiseInfo info)
        {
            var result = Browser.Put(AdvertisePath(endpoint), with =>
            {
                with.HttpRequest();
                with.JsonBody(info);
            });
            return result;
        }

        protected BrowserResponse SendMatchInfo(string endpoint, string timestamp, MatchInfo info)
        {
            var result = Browser.Put(MatchInfoPath(endpoint, timestamp), with =>
            {
                with.HttpRequest();
                with.JsonBody(info);
            });
            return result;
        }

        protected void SendStatsTestData()
        {
            //2 servers
            SendAdvertise(Endpoints[0], Advertises[0]);
            SendAdvertise(Endpoints[1], Advertises[1]);
            //7 matches on 1st server
            for (var i = 0; i < 7; ++i)
            {
                SendMatchInfo(Endpoints[0], Timestamps[i], Matches[i]);
            }
            //1 match on 2nd server
            SendMatchInfo(Endpoints[1], Timestamps[7], Matches[7]);
        }

        protected IEnumerable<RecentMatchesItem> SendRecentMatchesReportTestData()
        {
            var date = new DateTime();
            var endpoints = new List<string>();
            foreach (var advertiseInfo in Advertises)
            {
                var endpoint = $"{advertiseInfo.Name}-1111";
                endpoints.Add(endpoint);
                SendAdvertise(endpoint, advertiseInfo);
            }
            var matches = new List<RecentMatchesItem>();
            for (var i = 0; i < 55; ++i)
            {
                var match = Matches[i % Matches.Count];
                var endpoint = endpoints[i % endpoints.Count];
                var timestamp = date.ToString("yyyy-MM-ddTHH:mm:ssZ");
                matches.Add(new RecentMatchesItem
                {
                    Results = match,
                    Server = endpoint,
                    Timestamp = timestamp
                });
                SendMatchInfo(endpoint, timestamp, match);
                date = date.AddDays(1);
            }
            matches.Reverse();
            return matches;
        }


        protected IEnumerable<BestPlayersItem> SendBestPlayerReportTestData()
        {
            var bigKdaPlayer = new PlayerMatchInfo
            {
                Deaths = 1,
                Frags = 1000,
                Kills = 1000,
                Name = "KDA1000"
            };
            var noDeathsPlayer = new PlayerMatchInfo
            {
                Deaths = 0,
                Frags = 200,
                Kills = 200,
                Name = "no deaths 200"
            };
            var otherPlayers = new List<PlayerMatchInfo>();
            for (var i = 0; i < 55; ++i)
            {
                otherPlayers.Add(new PlayerMatchInfo
                {
                    Deaths = 2,
                    Frags = i * 10,
                    Kills = i * 10,
                    Name = $"just{i}"
                });
            }
            SendAdvertise(Endpoints[0], Advertises[0]);
            var date = new DateTime();
            //this players have only 5 matches: they won't in report
            for (var i = 0; i < 5; ++i)
            {
                var match = new MatchInfo
                {
                    Map = "kek",
                    GameMode = "DM",
                    FragLimit = 20,
                    TimeLimit = 20,
                    TimeElapsed = 12.345678,
                    Scoreboard = otherPlayers.Take(5).Concat(new List<PlayerMatchInfo>
                    {
                        bigKdaPlayer, noDeathsPlayer
                    }).ToList()
                };
                SendMatchInfo(Endpoints[0], date.ToString("yyyy-MM-ddTHH:mm:ssZ"), match);
                date = date.AddDays(1);
            }
            //all except noDeathsPlayer will be in report
            for (var i = 0; i < 50; ++i)
            {
                var match = new MatchInfo
                {
                    Map = "kek",
                    GameMode = "DM",
                    FragLimit = 20,
                    TimeLimit = 20,
                    TimeElapsed = 12.345678,
                    Scoreboard = otherPlayers.Skip(5).Concat(new List<PlayerMatchInfo>
                    {
                        noDeathsPlayer
                    }).ToList()
                };
                SendMatchInfo(Endpoints[0], date.ToString("yyyy-MM-ddTHH:mm:ssZ"), match);
                date = date.AddDays(1);
            }
            return otherPlayers
                .Select(p => new BestPlayersItem
                {
                    Name = p.Name,
                    KillToDeathRatio = (double)p.Kills / p.Deaths
                })
                .Reverse()
                .ToList();
        }

        protected IEnumerable<PopularServersItem> SendPopularServersReportTestData()
        {
            var date = new DateTime();
            var endpoints = new List<string>();
            var matches = new List<PopularServersItem>();
            for (var i = 0; i < 55; ++i)
            {
                var endpoint = $"server-{i}";
                endpoints.Add(endpoint);
                SendAdvertise(endpoint, new AdvertiseInfo
                {
                    Name = endpoint,
                    GameModes = new List<string> { "DM" }
                });
                matches.Add(new PopularServersItem
                {
                    AverageMatchesPerDay = 1.5 * i,
                    Endpoint = endpoint,
                    Name = endpoint
                });
            }
            for (var i = 0; i < 55; ++i)
            {
                for (var j = 0; j < i; ++j)
                {
                    SendMatchInfo(endpoints[i], date.ToString("yyyy-MM-ddTHH:mm:ssZ"), Matches[0]);
                    date = date.AddMinutes(1);
                    SendMatchInfo(endpoints[i], date.ToString("yyyy-MM-ddTHH:mm:ssZ"), Matches[1]);
                    date = date.AddMinutes(1);
                }
                date = date.AddDays(1);
                for (var j = 0; j < i; ++j)
                {
                    SendMatchInfo(endpoints[i], date.AddDays(1).ToString("yyyy-MM-ddTHH:mm:ssZ"), Matches[2]);
                    date = date.AddMinutes(1);
                }
                date = date.Date;
            }
            matches.Reverse();
            return matches;
        }

        #region TestData

        protected readonly List<AdvertiseInfo> Advertises = new List<AdvertiseInfo>
        {
            new AdvertiseInfo
            {
                Name = "Server",
                GameModes = new List<string> { "DM", "TDM", "CTF" }
            },
            new AdvertiseInfo
            {
                Name = "Server2",
                GameModes = new List<string> { "35hp", "ZE" }
            },
            new AdvertiseInfo
            {
                Name = "3serv",
                GameModes = new List<string> { "control point" }
            }
        };

        protected readonly List<string> Endpoints = new List<string>
        {
            "hostname1-990",
            "192.168.124.255-65500"
        };

        protected readonly List<string> Timestamps = new List<string>
        {
            "2017-01-22T15:17:00Z",
            "2017-01-22T15:16:00Z",
            "2017-01-23T15:16:00Z",
            "2017-02-13T15:16:01Z",
            "2017-02-13T15:16:02Z",
            "2017-02-13T15:16:03Z",
            "2017-02-13T15:16:04Z",
            "2017-02-13T15:16:05Z"
        };

        protected readonly List<MatchInfo> Matches = new List<MatchInfo>
        {
            new MatchInfo
            {
                Map = "kek",
                GameMode = "DM",
                FragLimit = 20,
                TimeLimit = 20,
                TimeElapsed = 12.345678,
                Scoreboard = new List<PlayerMatchInfo>
                {
                    new PlayerMatchInfo
                    {
                        Name = "P1",
                        Frags = 20,
                        Kills = 21,
                        Deaths = 3
                    },
                    new PlayerMatchInfo
                    {
                        Name = "P2",
                        Frags = 2,
                        Kills = 2,
                        Deaths = 21
                    }
                }
            },
            new MatchInfo
            {
                Map = "kek",
                GameMode = "CTF",
                FragLimit = 20,
                TimeLimit = 20,
                TimeElapsed = 12.345678,
                Scoreboard = new List<PlayerMatchInfo>
                {
                    new PlayerMatchInfo
                    {
                        Name = "P13",
                        Frags = 20,
                        Kills = 21,
                        Deaths = 3
                    },
                    new PlayerMatchInfo
                    {
                        Name = "P2",
                        Frags = 2,
                        Kills = 2,
                        Deaths = 21
                    },
                    new PlayerMatchInfo
                    {
                        Name = "P3",
                        Frags = 2,
                        Kills = 2,
                        Deaths = 21
                    },
                    new PlayerMatchInfo
                    {
                        Name = "P4",
                        Frags = 2,
                        Kills = 2,
                        Deaths = 21
                    },
                    new PlayerMatchInfo
                    {
                        Name = "P5",
                        Frags = 2,
                        Kills = 2,
                        Deaths = 21
                    }
                }
            },
            new MatchInfo
            {
                Map = "kek",
                GameMode = "CTF",
                FragLimit = 20,
                TimeLimit = 20,
                TimeElapsed = 12.345678,
                Scoreboard = new List<PlayerMatchInfo>
                {
                    new PlayerMatchInfo
                    {
                        Name = "P14",
                        Frags = 20,
                        Kills = 21,
                        Deaths = 3
                    },
                    new PlayerMatchInfo
                    {
                        Name = "P2",
                        Frags = 2,
                        Kills = 2,
                        Deaths = 21
                    },
                    new PlayerMatchInfo
                    {
                        Name = "P3",
                        Frags = 2,
                        Kills = 2,
                        Deaths = 21
                    }
                }
            },
            new MatchInfo
            {
                Map = "CP1",
                GameMode = "CP",
                FragLimit = 20,
                TimeLimit = 20,
                TimeElapsed = 12.345678,
                Scoreboard = new List<PlayerMatchInfo>
                {
                    new PlayerMatchInfo
                    {
                        Name = "P1",
                        Frags = 20,
                        Kills = 12,
                        Deaths = 34
                    },
                    new PlayerMatchInfo
                    {
                        Name = "P3",
                        Frags = 2,
                        Kills = 2,
                        Deaths = 21
                    },
                    new PlayerMatchInfo
                    {
                        Name = "P4",
                        Frags = 2,
                        Kills = 2,
                        Deaths = 21
                    }
                }
            },
            new MatchInfo
            {
                Map = "kek",
                GameMode = "CTF",
                FragLimit = 20,
                TimeLimit = 20,
                TimeElapsed = 12.345678,
                Scoreboard = new List<PlayerMatchInfo>
                {
                    new PlayerMatchInfo
                    {
                        Name = "P122",
                        Frags = 20,
                        Kills = 21,
                        Deaths = 3
                    },
                    new PlayerMatchInfo
                    {
                        Name = "P1",
                        Frags = 2,
                        Kills = 2,
                        Deaths = 21
                    },
                    new PlayerMatchInfo
                    {
                        Name = "P3",
                        Frags = 2,
                        Kills = 2,
                        Deaths = 21
                    }
                }
            },
            new MatchInfo
            {
                Map = "kek",
                GameMode = "DM",
                FragLimit = 20,
                TimeLimit = 20,
                TimeElapsed = 12.345678,
                Scoreboard = new List<PlayerMatchInfo>
                {
                    new PlayerMatchInfo
                    {
                        Name = "P2",
                        Frags = 2,
                        Kills = 2,
                        Deaths = 21
                    },
                    new PlayerMatchInfo
                    {
                        Name = "p1",
                        Frags = 20,
                        Kills = 21,
                        Deaths = 3
                    }
                }
            },
            new MatchInfo
            {
                Map = "kek",
                GameMode = "CTF",
                FragLimit = 20,
                TimeLimit = 20,
                TimeElapsed = 12.345678,
                Scoreboard = new List<PlayerMatchInfo>
                {
                    new PlayerMatchInfo
                    {
                        Name = "P1",
                        Frags = 20,
                        Kills = 21,
                        Deaths = 3
                    },
                    new PlayerMatchInfo
                    {
                        Name = "P2",
                        Frags = 2,
                        Kills = 2,
                        Deaths = 21
                    }
                }
            },
            new MatchInfo
            {
                Map = "kek",
                GameMode = "CTF",
                FragLimit = 20,
                TimeLimit = 20,
                TimeElapsed = 12.345678,
                Scoreboard = new List<PlayerMatchInfo>
                {
                    new PlayerMatchInfo
                    {
                        Name = "P1",
                        Frags = 20,
                        Kills = 21,
                        Deaths = 3
                    },
                    new PlayerMatchInfo
                    {
                        Name = "P2",
                        Frags = 2,
                        Kills = 2,
                        Deaths = 21
                    }
                }
            }
        };

        protected readonly ServerStatsInfo ServerStats = new ServerStatsInfo
        {
            AverageMatchesPerDay = 7.0 / 3,
            AveragePopulation = 20 / 7.0,
            MaximumMatchesPerDay = 4,
            MaximumPopulation = 5,
            Name = "Server",
            TotalMatchesPlayed = 7,
            Top5GameModes = new List<string>
            {
                "CTF",
                "DM",
                "CP"
            },
            Top5Maps = new List<string>
            {
                "kek",
                "CP1"
            }
        };

        //stats of "p1" player
        protected readonly PlayerStatsInfo PlayerStats = new PlayerStatsInfo
        {
            TotalMatchesPlayed = 6,
            TotalMatchesWon = 4,
            FavoriteServer = "hostname1-990",
            UniqueServers = 2,
            FavoriteGameMode = "CTF",
            AverageScoreboardPercent = 75,
            MaximumMatchesPerDay = 5,
            AverageMatchesPerDay = 3,
            LastMatchPlayed = "2017-02-13T15:16:05Z",
            KillToDeathRatio = 98 / 67.0
        };

        #endregion
    }
}