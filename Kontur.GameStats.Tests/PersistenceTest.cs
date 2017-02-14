using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using Kontur.GameStats.Server;
using Kontur.GameStats.Server.Data;
using Nancy.Testing;
using NUnit.Framework;

namespace Kontur.GameStats.Tests
{
    [TestFixture]
    public class PersistenceTest : NancyTest
    {
        private const string DbName = "PersistenceTest";

        [OneTimeSetUp]
        public void DeleteDb()
        {
            File.Delete($"{DbName}.db");
            File.Delete($"{DbName}-journal.db");
        }

        [SetUp]
        public void CreateModuleAndConnect()
        {
            Bootstrapper = new NancyBootstrapper(DbName);
            Browser = new Browser(Bootstrapper);
        }

        [TearDown]
        public void TearDown()
        {
            DisposeModule();
            DeleteDb();
        }

        private void DisposeModule()
        {
            Bootstrapper.Dispose();
        }

        [Test]
        public void StatModule_ShouldReturnAdvertiseInfo_AfterDisposing()
        {
            SendAdvertise(Endpoints[0], Advertises[0]);

            DisposeModule();
            CreateModuleAndConnect();

            var result = Get(AdvertisePath(Endpoints[0]));
            var recievedInfo = result.Body.DeserializeJson<AdvertiseInfo>();

            recievedInfo.ShouldBeEquivalentTo(Advertises[0]);
        }

        [Test]
        public void StatModule_ShouldReturnMatchInfo_AfterDisposing()
        {
            SendAdvertise(Endpoints[0], Advertises[0]);
            SendMatchInfo(Endpoints[0], Timestamps[0], Matches[0]);

            DisposeModule();
            CreateModuleAndConnect();

            var result = Get(MatchInfoPath(Endpoints[0], Timestamps[0]));
            var info = result.Body.DeserializeJson<MatchInfo>();

            info.ShouldBeEquivalentTo(Matches[0]);
        }

        [Test]
        public void StatModule_ShouldReturnAllServersInfo_AfterDisposing()
        {
            SendAdvertise(Endpoints[0], Advertises[0]);
            SendAdvertise(Endpoints[0], Advertises[1]);
            SendAdvertise(Endpoints[1], Advertises[2]);

            DisposeModule();
            CreateModuleAndConnect();

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
        public void StatModule_ShouldReturnServerStats_AfterDisposing()
        {
            SendStatsTestData();

            DisposeModule();
            CreateModuleAndConnect();

            var result = Get(ServerStatsPath(Endpoints[0]));
            var info = result.Body.DeserializeJson<ServerStatsInfo>();

            info.ShouldBeEquivalentTo(ServerStats, options => options.WithStrictOrdering());
        }

        [Test]
        public void StatModule_ShouldReturnPlayerStats_AfterDisposing()
        {
            SendStatsTestData();

            DisposeModule();
            CreateModuleAndConnect();

            var result = Get(PlayerStatsPath("p1"));
            var info = result.Body.DeserializeJson<PlayerStatsInfo>();

            info.ShouldBeEquivalentTo(PlayerStats);
        }

        [Test]
        public void StatModule_ShouldReturnRecentMatches_AfterDisposing()
        {
            var matches = SendRecentMatchesReportTestData();

            DisposeModule();
            CreateModuleAndConnect();

            var result = Get(RecentMatchesPath(42));
            var recievedMatches = result.Body.DeserializeJson<List<RecentMatchesItem>>();

            recievedMatches.ShouldAllBeEquivalentTo(matches.Take(42), options => options.WithStrictOrdering());
        }

        [Test]
        public void StatModule_ShouldReturnBestPlayers_AfterDisposing()
        {
            var matches = SendBestPlayerReportTestData();

            DisposeModule();
            CreateModuleAndConnect();

            var result = Get(BestPlayersPath(42));
            var recievedMatches = result.Body.DeserializeJson<List<BestPlayersItem>>();
            recievedMatches.ShouldAllBeEquivalentTo(matches.Take(42), options => options.WithStrictOrdering());
        }

        [Test]
        public void StatModule_ShouldReturnPopularServers_AfterDisposing()
        {
            var matches = SendPopularServersReportTestData();

            DisposeModule();
            CreateModuleAndConnect();

            var result = Get(PopularServersPath(42));
            var recievedMatches = result.Body.DeserializeJson<List<PopularServersItem>>();
            recievedMatches.ShouldAllBeEquivalentTo(matches.Take(42), options => options.WithStrictOrdering());
        }
    }
}

