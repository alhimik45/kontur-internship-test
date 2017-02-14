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
        private readonly LiteDatabase _db;
        private readonly int _maxReportSize;

        private readonly PersistentDictionary<string, AdvertiseInfo> _servers;
        private readonly PersistentDictionary<Pair<string, string>, MatchInfo> _matches;
        private readonly PersistentDictionary<string, ServerStatsInfo> _stats;
        private readonly PersistentDictionary<string, InternalServerStats> _internalStats;
        private readonly List<RecentMatchesItem> _recentMatches;
        private readonly List<PopularServersItem> _popularServers;

        private readonly LiteCollection<DbEntry<int, RecentMatchesItem>> _recentMatchesColl;
        private readonly LiteCollection<DbEntry<int, PopularServersItem>> _popularServersColl;

        public ServerStatistics(LiteDatabase db, int maxReportSize)
        {
            _db = db;
            _maxReportSize = maxReportSize;

            _servers =  new PersistentDictionary<string, AdvertiseInfo>(db, "Servers");
            _matches =  new PersistentDictionary<Pair<string, string>, MatchInfo>(db, "Matches");
            _internalStats =  new PersistentDictionary<string, InternalServerStats>(db, "InternalServerStats");
            _stats =  new PersistentDictionary<string, ServerStatsInfo>(db, "ServerStatsI");

            _recentMatchesColl = _db.GetCollection<DbEntry<int, RecentMatchesItem>>("RecentMatches");
            _recentMatches = _recentMatchesColl.FindAll().OrderBy(e => e.Id).Select(e => e.Value).ToList();

            _popularServersColl = _db.GetCollection<DbEntry<int, PopularServersItem>>("PopularServers");
            _popularServers = _popularServersColl.FindAll().OrderBy(e => e.Id).Select(e => e.Value).ToList();
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
            using (var transaction = _db.BeginTrans())
            {
                CalcStats(endpoint, timestamp, info);
                transaction.Commit();
            }
            _matches[Pair.Create(endpoint, timestamp)] = info;
        }

        public MatchInfo GetMatch(string endpoint, string timestamp)
        {
            return _matches.Get(Pair.Create(endpoint, timestamp));
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

            var newStats = oldStats.CalcNew(_servers[endpoint].Name, info, internalStats);

            UpdateRecentMatchesReport(endpoint, timestamp, info);
            UpdatePopularServersReport(endpoint, newStats);

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
            for (var i = 0; i < _recentMatches.Count; i++)
            {
                _recentMatchesColl.Upsert(i, new DbEntry<int, RecentMatchesItem>(i, _recentMatches[i]));
            }
        }

        private void UpdatePopularServersReport(string endpoint, ServerStatsInfo info)
        {
            _popularServers.UpdateTop(_maxReportSize,
                ps => ps.AverageMatchesPerDay,
                ps => ps.Name,
                new PopularServersItem
                {
                    Endpoint = endpoint,
                    Name = info.Name,
                    AverageMatchesPerDay = info.AverageMatchesPerDay
                });
            for (var i = 0; i < _popularServers.Count; i++)
            {
                _popularServersColl.Upsert(i, new DbEntry<int, PopularServersItem>(i, _popularServers[i]));
            }
        }
    }
}