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
    public class StatModuleShould : NancyTest
    {
        [SetUp]
        public void SetUp()
        {
            DeleteData();
            Bootstrapper = new NancyBootstrapper();
            Browser = new Browser(Bootstrapper);
        }

        [TearDown]
        public void TearDown()
        {
            Bootstrapper.Dispose();
        }

        [Test]
        public void returnOK_onCorrectAdvertiseInfo()
        {
            var result = SendAdvertise(Endpoints[0], Advertises[0]);
            result.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Test]
        public void returnBadRequest_onCorrectAdvertiseInfo_withInvalidEndpoint()
        {
            var result = SendAdvertise("322-wrong", Advertises[0]);
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Test]
        public void returnBadRequest_onWrongAdvertiseInfo()
        {
            var result = Browser.Put(AdvertisePath(Endpoints[0]), with =>
            {
                with.HttpRequest();
                with.Body("{}", "application/json");
            });
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Test]
        public void returnOK_onCorrectMatchInfo_afterAdvertise()
        {
            SendAdvertise(Endpoints[0], Advertises[0]);
            var result = SendMatchInfo(Endpoints[0], Timestamps[0], Matches[0]);
            result.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Test]
        public void returnBadRequest_onCorrectMatchInfo_afterAdvertise_withInvalidTimestamp()
        {
            SendAdvertise(Endpoints[0], Advertises[0]);
            var result = SendMatchInfo(Endpoints[0], "wrongTS", Matches[0]);
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Test]
        public void returnBadRequest_onMatchInfo_afterAdvertise_withInvalidEndpoint()
        {
            SendAdvertise(Endpoints[0], Advertises[0]);
            var result = SendMatchInfo("wrongendpoint", Timestamps[0], Matches[0]);
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Test]
        public void returnBadRequest_onWrongMatchInfo_afterAdvertise()
        {
            SendAdvertise(Endpoints[0], Advertises[0]);
            var result = Browser.Put(MatchInfoPath(Endpoints[0], Timestamps[0]), with =>
            {
                with.HttpRequest();
                with.Body("{}", "application/json");
            });
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Test]
        public void returnBadRequest_onCorrectMatchInfo_withoutAdvertise()
        {
            var result = SendMatchInfo(Endpoints[0], Timestamps[0], Matches[0]);
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Test]
        public void returnSameInfo_onGettingServerInfo_afterAdvertise()
        {
            SendAdvertise(Endpoints[0], Advertises[0]);
            var result = Get(AdvertisePath(Endpoints[0]));
            var recievedInfo = result.Body.DeserializeJson<AdvertiseInfo>();
            recievedInfo.ShouldBeEquivalentTo(Advertises[0]);
        }

        [Test]
        public void returnNotFound_onGettingServerInfo_withoutAdvertise()
        {
            var result = Get(AdvertisePath(Endpoints[0]));
            result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Test]
        public void returnEmptyList_onGetingAllServers_withoutAnyAdvertise()
        {
            var result = Get(ServersInfoPath);
            var list = result.Body.DeserializeJson<List<ServersInfoItem>>();
            list.Should().HaveCount(0);
        }

        [Test]
        public void returnEmptyList_onGetingAllServers_withoutAdvertises()
        {
            var result = Get(ServersInfoPath);
            var list = result.Body.DeserializeJson<List<ServersInfoItem>>();

            list.Should().HaveCount(0);
        }

        [Test]
        public void returnList_onGetingAllServers_afterAdvertises()
        {
            SendAdvertise(Endpoints[0], Advertises[0]);
            SendAdvertise(Endpoints[0], Advertises[1]);
            SendAdvertise(Endpoints[1], Advertises[2]);
            var result = Get(ServersInfoPath);
            var list = result.Body.DeserializeJson<List<ServersInfoItem>>();

            list.ShouldAllBeEquivalentTo(new List<ServersInfoItem>
            {
                new ServersInfoItem
                {
                    Endpoint = Endpoints[0],
                    Info = Advertises[1]
                },
                new ServersInfoItem
                {
                    Endpoint = Endpoints[1],
                    Info = Advertises[2]
                }
            });
        }

        [Test]
        public void returnMatchInfo_onGetingMatchInfo_afterPuttingIt()
        {
            SendAdvertise(Endpoints[0], Advertises[0]);
            SendMatchInfo(Endpoints[0], Timestamps[0], Matches[0]);

            var result = Get(MatchInfoPath(Endpoints[0], Timestamps[0]));
            var info = result.Body.DeserializeJson<MatchInfo>();

            info.ShouldBeEquivalentTo(Matches[0]);
        }

        [Test]
        public void returnNotFound_onGetingMatchInfo_withoutPuttingIt()
        {
            SendAdvertise(Endpoints[0], Advertises[0]);
            var result = Get(MatchInfoPath(Endpoints[0], Timestamps[0]));

            result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Test]
        public void returnStats_onServerStatsRequest_afterSendingData()
        {
            SendStatsTestData();
            var result = Get(ServerStatsPath(Endpoints[0]));
            var info = result.Body.DeserializeJson<PublicServerStats>();
            info.ShouldBeEquivalentTo(ServerStats, options => options.WithStrictOrdering());
        }

        [Test]
        public void returnNotFound_onServerStatsRequest_withoutSendingData()
        {
            var result = Get(ServerStatsPath(Endpoints[0]));
            result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Test]
        public void returnStats_onPlayerStatsRequest_afterSendingData()
        {
            SendStatsTestData();
            var result = Get(PlayerStatsPath("p1"));
            var info = result.Body.DeserializeJson<PublicPlayerStats>();
            info.ShouldBeEquivalentTo(PlayerStats);
        }

        [Test]
        public void return100Scoreboard_onPlayerStatsRequest_withOnePlayer()
        {
            SendAdvertise(Endpoints[0], Advertises[0]);
            SendMatchInfo(Endpoints[0], Timestamps[0], new MatchInfo
            {
                GameMode = "gm",
                FragLimit = 1,
                Map = "ff",
                TimeElapsed = 10,
                TimeLimit = 10,
                Scoreboard = new List<PlayerMatchInfo>
                {
                    new PlayerMatchInfo
                    {
                        Name = "p1",
                        Deaths = 1,
                        Frags = 1,
                        Kills = 12
                    }
                }
            });
            var result = Get(PlayerStatsPath("p1"));
            var info = result.Body.DeserializeJson<PublicPlayerStats>();
            info.AverageScoreboardPercent.Should().Be(100);
        }

        [Test]
        public void returnNotFound_onPlayerStatsRequest_withoutSendingData()
        {
            var result = Get(PlayerStatsPath("p1"));
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
            var result = Get(RecentMatchesPath(sentCount));
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
            var result = Get(BestPlayersPath(sentCount));
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
            var result = Get(PopularServersPath(sentCount));
            var recievedMatches = result.Body.DeserializeJson<List<PopularServersItem>>();
            recievedMatches.ShouldAllBeEquivalentTo(matches.Take(checkCount), options => options.WithStrictOrdering());
        }
    }
}

