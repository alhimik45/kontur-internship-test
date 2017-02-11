using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Kontur.GameStats.Server;
using Kontur.GameStats.Server.Data;
using Nancy;
using Nancy.Testing;
using NUnit.Framework;

namespace Kontur.GameStats.Tests
{
    [TestFixture]
    public class StatModuleShould
    {
        private Browser _browser;

        private readonly string _serversInfoPath = "/servers/info";
        private readonly Func<int?, string> _recentMatchesPath = count => "/reports/recent-matches" + (count.HasValue ? $"/{count.Value}" : "");
        private readonly Func<int?, string> _bestPlayersPath = count => "/reports/best-players" + (count.HasValue ? $"/{count.Value}" : "");
        private readonly Func<int?, string> _popularServersPath = count => "/reports/popular-servers" + (count.HasValue ? $"/{count.Value}" : "");
        private readonly Func<string, string> _serverStatsPath = endpoint => $"/servers/{endpoint}/stats";
        private readonly Func<string, string> _playerStatsPath = name => $"/players/{name}/stats";
        private readonly Func<string, string> _advertisePath = endpoint => $"/servers/{endpoint}/info";
        private readonly Func<string, string, string> _matchInfoPath = (endpoint, timestamp) => $"/servers/{endpoint}/matches/{timestamp}";

        [SetUp]
        public void SetUp()
        {
            _browser = new Browser(new NancyBootstrapper());
        }

        private BrowserResponse Get(string path)
        {
            var result = _browser.Get(path, with =>
            {
                with.HttpRequest();
            });
            return result;
        }

        private BrowserResponse SendAdvertise(string endpoint, AdvertiseInfo info)
        {
            var result = _browser.Put(_advertisePath(endpoint), with =>
            {
                with.HttpRequest();
                with.JsonBody(info);
            });
            return result;
        }

        private BrowserResponse SendMatchInfo(string endpoint, string timestamp, MatchInfo info)
        {
            var result = _browser.Put(_matchInfoPath(endpoint, timestamp), with =>
             {
                 with.HttpRequest();
                 with.JsonBody(info);
             });
            return result;
        }

        private void SendStatsTestData()
        {
            //2 servers
            SendAdvertise(TestData.Endpoints[0], TestData.Advertises[0]);
            SendAdvertise(TestData.Endpoints[1], TestData.Advertises[1]);
            //7 matches on 1st server
            for (var i = 0; i < 7; ++i)
            {
                SendMatchInfo(TestData.Endpoints[0], TestData.Timestamps[i], TestData.Matches[i]);
            }
            //1 match on 2nd server
            SendMatchInfo(TestData.Endpoints[1], TestData.Timestamps[7], TestData.Matches[7]);
        }

        private List<RecentMatchesItem> SendRecentMatchesReportTestData()
        {
            var date = new DateTime();
            var endpoints = new List<string>();
            foreach (var advertiseInfo in TestData.Advertises)
            {
                var endpoint = $"{advertiseInfo.Name}-1111";
                endpoints.Add(endpoint);
                SendAdvertise(endpoint, advertiseInfo);
            }
            var matches = new List<RecentMatchesItem>();
            for (var i = 0; i < 55; ++i)
            {
                var match = TestData.Matches[i % TestData.Matches.Count];
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


        private List<BestPlayersItem> SendBestPlayerReportTestData()
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
            SendAdvertise(TestData.Endpoints[0], TestData.Advertises[0]);
            var date = new DateTime();
            //only 5 matches: they won't in report
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
                SendMatchInfo(TestData.Endpoints[0], date.ToString("yyyy-MM-ddTHH:mm:ssZ"), match);
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
                SendMatchInfo(TestData.Endpoints[0], date.ToString("yyyy-MM-ddTHH:mm:ssZ"), match);
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

        private List<PopularServersItem> SendPopularServersReportTestData()
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
                    SendMatchInfo(endpoints[i], date.ToString("yyyy-MM-ddTHH:mm:ssZ"), TestData.Matches[0]);
                    date = date.AddMinutes(1);
                    SendMatchInfo(endpoints[i], date.ToString("yyyy-MM-ddTHH:mm:ssZ"), TestData.Matches[1]);
                    date = date.AddMinutes(1);
                }
                date = date.AddDays(1);
                for (var j = 0; j < i; ++j)
                {
                    SendMatchInfo(endpoints[i], date.AddDays(1).ToString("yyyy-MM-ddTHH:mm:ssZ"), TestData.Matches[2]);
                    date = date.AddMinutes(1);
                }
                date = date.Date;
            }
            matches.Reverse();
            return matches;
        }

        [Test]
        public void returnOK_onCorrectAdvertiseInfo()
        {
            var result = SendAdvertise(TestData.Endpoints[0], TestData.Advertises[0]);
            result.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Test]
        public void returnBadRequest_onCorrectAdvertiseInfo_withInvalidEndpoint()
        {
            var result = SendAdvertise("322-wrong", TestData.Advertises[0]);
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Test]
        public void returnBadRequest_onWrongAdvertiseInfo()
        {
            var result = _browser.Put(_advertisePath(TestData.Endpoints[0]), with =>
            {
                with.HttpRequest();
                with.Body("{}", "application/json");
            });
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Test]
        public void returnOK_onCorrectMatchInfo_afterAdvertise()
        {
            SendAdvertise(TestData.Endpoints[0], TestData.Advertises[0]);
            var result = SendMatchInfo(TestData.Endpoints[0], TestData.Timestamps[0], TestData.Matches[0]);
            result.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Test]
        public void returnBadRequest_onCorrectMatchInfo_afterAdvertise_withInvalidTimestamp()
        {
            SendAdvertise(TestData.Endpoints[0], TestData.Advertises[0]);
            var result = SendMatchInfo(TestData.Endpoints[0], "wrongTS", TestData.Matches[0]);
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Test]
        public void returnBadRequest_onMatchInfo_afterAdvertise_withInvalidEndpoint()
        {
            SendAdvertise(TestData.Endpoints[0], TestData.Advertises[0]);
            var result = SendMatchInfo("wrongendpoint", TestData.Timestamps[0], TestData.Matches[0]);
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Test]
        public void returnBadRequest_onWrongMatchInfo_afterAdvertise()
        {
            SendAdvertise(TestData.Endpoints[0], TestData.Advertises[0]);
            var result = _browser.Put(_matchInfoPath(TestData.Endpoints[0], TestData.Timestamps[0]), with =>
            {
                with.HttpRequest();
                with.Body("{}", "application/json");
            });
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Test]
        public void returnBadRequest_onCorrectMatchInfo_withoutAdvertise()
        {
            var result = SendMatchInfo(TestData.Endpoints[0], TestData.Timestamps[0], TestData.Matches[0]);
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Test]
        public void returnSameInfo_onGettingServerInfo_afterAdvertise()
        {
            SendAdvertise(TestData.Endpoints[0], TestData.Advertises[0]);
            var result = Get(_advertisePath(TestData.Endpoints[0]));
            var recievedInfo = result.Body.DeserializeJson<AdvertiseInfo>();
            recievedInfo.ShouldBeEquivalentTo(TestData.Advertises[0]);
        }

        [Test]
        public void returnNotFound_onGettingServerInfo_withoutAdvertise()
        {
            var result = Get(_advertisePath(TestData.Endpoints[0]));
            result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Test]
        public void returnEmptyList_onGetingAllServers_withoutAnyAdvertise()
        {
            var result = Get(_serversInfoPath);
            var list = result.Body.DeserializeJson<List<ServersInfoItem>>();
            list.Should().HaveCount(0);
        }

        [Test]
        public void returnEmptyList_onGetingAllServers_withoutAdvertises()
        {
            var result = Get(_serversInfoPath);
            var list = result.Body.DeserializeJson<List<ServersInfoItem>>();

            list.Should().HaveCount(0);
        }

        [Test]
        public void returnList_onGetingAllServers_afterAdvertises()
        {
            SendAdvertise(TestData.Endpoints[0], TestData.Advertises[0]);
            SendAdvertise(TestData.Endpoints[0], TestData.Advertises[1]);
            SendAdvertise(TestData.Endpoints[1], TestData.Advertises[2]);
            var result = Get(_serversInfoPath);
            var list = result.Body.DeserializeJson<List<ServersInfoItem>>();

            list.ShouldAllBeEquivalentTo(new List<ServersInfoItem>
            {
                new ServersInfoItem
                {
                    Endpoint = TestData.Endpoints[0],
                    Info = TestData.Advertises[1]
                },
                new ServersInfoItem
                {
                    Endpoint = TestData.Endpoints[1],
                    Info = TestData.Advertises[2]
                }
            });
        }

        [Test]
        public void returnMatchInfo_onGetingMatchInfo_afterPuttingIt()
        {
            SendAdvertise(TestData.Endpoints[0], TestData.Advertises[0]);
            SendMatchInfo(TestData.Endpoints[0], TestData.Timestamps[0], TestData.Matches[0]);

            var result = Get(_matchInfoPath(TestData.Endpoints[0], TestData.Timestamps[0]));

            var info = result.Body.DeserializeJson<MatchInfo>();

            info.ShouldBeEquivalentTo(TestData.Matches[0]);
        }

        [Test]
        public void returnNotFound_onGetingMatchInfo_withoutPuttingIt()
        {
            SendAdvertise(TestData.Endpoints[0], TestData.Advertises[0]);
            var result = Get(_matchInfoPath(TestData.Endpoints[0], TestData.Timestamps[0]));

            result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Test]
        public void returnStats_onServerStatsRequest_afterSendingData()
        {
            SendStatsTestData();
            var result = Get(_serverStatsPath(TestData.Endpoints[0]));
            var info = result.Body.DeserializeJson<ServerStatsInfo>();
            info.ShouldBeEquivalentTo(TestData.ServerStats, options => options.WithStrictOrdering());
        }

        [Test]
        public void returnNotFound_onServerStatsRequest_withoutSendingData()
        {
            var result = Get(_serverStatsPath(TestData.Endpoints[0]));
            result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Test]
        public void returnStats_onPlayerStatsRequest_afterSendingData()
        {
            SendStatsTestData();
            var result = Get(_playerStatsPath("p1"));
            var info = result.Body.DeserializeJson<PlayerStatsInfo>();
            info.ShouldBeEquivalentTo(TestData.PlayerStats);
        }

        [Test]
        public void returnNotFound_onPlayerStatsRequest_withoutSendingData()
        {
            var result = Get(_playerStatsPath("p1"));
            result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [TestCase(null, 5, TestName = "withoutExplicitCount")]
        [TestCase(42, 42, TestName = "withCount")]
        [TestCase(64, 50, TestName = "withCountMoreThan50")]
        [TestCase(0, 0, TestName = "withZeroCount")]
        [TestCase(-32, 0, TestName = "withNegativeCount")]
        public void returnList_onGettingRecentMatchesReport(int? sentCount, int checkCount)
        {
            var matches = SendRecentMatchesReportTestData();
            var result = Get(_recentMatchesPath(sentCount));
            var recievedMatches = result.Body.DeserializeJson<List<RecentMatchesItem>>();
            recievedMatches.ShouldAllBeEquivalentTo(matches.Take(checkCount), options => options.WithStrictOrdering());
        }

        [TestCase(null, 5, TestName = "withoutExplicitCount")]
        [TestCase(42, 42, TestName = "withCount")]
        [TestCase(64, 50, TestName = "withCountMoreThan50")]
        [TestCase(0, 0, TestName = "withZeroCount")]
        [TestCase(-32, 0, TestName = "withNegativeCount")]
        public void returnList_onGettingBestPlayersReport(int? sentCount, int checkCount)
        {
            var matches = SendBestPlayerReportTestData();
            var result = Get(_bestPlayersPath(sentCount));
            var recievedMatches = result.Body.DeserializeJson<List<BestPlayersItem>>();
            recievedMatches.ShouldAllBeEquivalentTo(matches.Take(checkCount), options => options.WithStrictOrdering());
        }

        [TestCase(null, 5, TestName = "withoutExplicitCount")]
        [TestCase(42, 42, TestName = "withCount")]
        [TestCase(64, 50, TestName = "withCountMoreThan50")]
        [TestCase(0, 0, TestName = "withZeroCount")]
        [TestCase(-32, 0, TestName = "withNegativeCount")]
        public void returnList_onGettingPopularServersReport(int? sentCount, int checkCount)
        {
            var matches = SendPopularServersReportTestData();
            var result = Get(_popularServersPath(sentCount));
            var recievedMatches = result.Body.DeserializeJson<List<PopularServersItem>>();
            recievedMatches.ShouldAllBeEquivalentTo(matches.Take(checkCount), options => options.WithStrictOrdering());
        }
    }
}

