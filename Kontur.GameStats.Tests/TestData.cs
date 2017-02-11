using System.Collections.Generic;
using Kontur.GameStats.Server.Data;

namespace Kontur.GameStats.Tests
{
    public static class TestData
    {
        public static List<AdvertiseInfo> Advertises = new List<AdvertiseInfo>
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

        public static List<string> Endpoints = new List<string>
        {
            "hostname1-990",
            "192.168.124.255-65500"
        };

        public static List<string> Timestamps = new List<string>
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

        public static List<MatchInfo> Matches = new List<MatchInfo>
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

        public static ServerStatsInfo ServerStats = new ServerStatsInfo
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

        public static PlayerStatsInfo PlayerStats = new PlayerStatsInfo
        {
            TotalMatchesPlayed = 6,
            TotalMatchesWon = 4,
            FavoriteServer = Endpoints[0],
            UniqueServers = 2,
            FavoriteGameMode = "CTF",
            AverageScoreboardPercent = 75,
            MaximumMatchesPerDay = 5,
            AverageMatchesPerDay = 3,
            LastMatchPlayed = "2017-02-13T15:16:05Z",
            KillToDeathRatio = 98 / 67.0
        };
    }
}