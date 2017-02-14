using System;
using System.Collections.Generic;
using System.Linq;
using Kontur.GameStats.Server.Data;
using Kontur.GameStats.Server.Extensions;
using Kontur.GameStats.Server.Util;
using LiteDB;

namespace Kontur.GameStats.Server.Logic
{
    public class ServerStatistics
    {
        private readonly int _maxReportSize;

        private readonly PersistentDictionary<string, AdvertiseInfo> _servers;
        private readonly PersistentDictionary<Pair<string, string>, MatchInfo> _matches;
        private readonly PersistentDictionary<string, ServerStatsInfo> _stats;
        private readonly PersistentDictionary<string, InternalServerStats> _internalStats;
        private readonly PersistentList<RecentMatchesItem> _recentMatches;
        private readonly PersistentList<PopularServersItem> _popularServers;

        public ServerStatistics(LiteDatabase db, int maxReportSize)
        {
            _maxReportSize = maxReportSize;

            _servers = new PersistentDictionary<string, AdvertiseInfo>(db, "Servers");
            _matches = new PersistentDictionary<Pair<string, string>, MatchInfo>(db, "Matches");
            _internalStats = new PersistentDictionary<string, InternalServerStats>(db, "InternalServerStats");
            _stats = new PersistentDictionary<string, ServerStatsInfo>(db, "ServerStats");
            _recentMatches = new PersistentList<RecentMatchesItem>(db, "RecentMatches");
            _popularServers = new PersistentList<PopularServersItem>(db, "PopularServers");
        }

        public void PutAdvertise(string endpoint, AdvertiseInfo info)
        {
            _servers[endpoint] = info;
        }

        public bool HasAdvertise(string endpoint)
        {
            return _servers.ContainsKey(endpoint);
        }

        public AdvertiseInfo GetAdvertise(string endpoint)
        {
            return _servers.Get(endpoint);
        }

        public List<ServersInfoItem> GetAll()
        {
            return _servers
                .Select(kv => new ServersInfoItem
                {
                    Endpoint = kv.Key,
                    Info = kv.Value
                })
                .ToList();
        }

        public void PutMatch(string endpoint, string timestamp, MatchInfo info)
        {
            _matches[Pair.Of(endpoint, timestamp)] = info;
            CalcStats(endpoint, timestamp, info);
        }

        public MatchInfo GetMatch(string endpoint, string timestamp)
        {
            return _matches.Get(Pair.Of(endpoint, timestamp));
        }

        public ServerStatsInfo GetStats(string endpoint)
        {
            return _stats.Get(endpoint);
        }

        public List<RecentMatchesItem> GetRecentMatches(int count)
        {
            return _recentMatches.Take(count).ToList();
        }

        public List<PopularServersItem> GetPopularServers(int count)
        {
            return _popularServers.Take(count).ToList();
        }

        private void CalcStats(string endpoint, string timestamp, MatchInfo info)
        {
            var time = timestamp.ToUtc();
            ServerStatsInfo oldStats;
            InternalServerStats internalStats;
            if (!_stats.TryGetValue(endpoint, out oldStats))
            {
                internalStats = new InternalServerStats { LastMatchDay = time.Date };
                _internalStats[endpoint] = internalStats;
                oldStats = new ServerStatsInfo();
            }
            else
            {
                internalStats = _internalStats[endpoint];
            }

            internalStats.Update(time, info);

            var serverName = _servers[endpoint].Name;

            var newStats = oldStats.CalcNew(serverName, info, internalStats);

            UpdateRecentMatchesReport(endpoint, timestamp, info);
            UpdatePopularServersReport(serverName, endpoint, newStats);

            _stats[endpoint] = newStats;
        }

        private void UpdateRecentMatchesReport(string endpoint, string timestamp, MatchInfo info)
        {
            _recentMatches.UpdateTop(_maxReportSize,
                rm => rm.Timestamp.ToUtc(),
                rm => Tuple.Create(rm.Timestamp, rm.Server),
                new RecentMatchesItem
                {
                    Server = endpoint,
                    Timestamp = timestamp,
                    Results = info
                });
        }

        private void UpdatePopularServersReport(string serverName, string endpoint, ServerStatsInfo info)
        {
            _popularServers.UpdateTop(_maxReportSize,
                ps => ps.AverageMatchesPerDay,
                ps => ps.Name,
                new PopularServersItem
                {
                    Endpoint = endpoint,
                    Name = serverName,
                    AverageMatchesPerDay = info.AverageMatchesPerDay
                });
        }
    }
}