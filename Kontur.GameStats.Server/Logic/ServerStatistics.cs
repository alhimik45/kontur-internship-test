using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kontur.GameStats.Server.Data;
using Kontur.GameStats.Server.Extensions;
using Kontur.GameStats.Server.Util;

namespace Kontur.GameStats.Server.Logic
{
    public class ServerStatistics
    {
        private readonly int _maxReportSize;

        private readonly PersistentDictionary<AdvertiseInfo> _servers;
        private readonly PersistentDictionary<MatchInfo> _matches;
        private readonly PersistentDictionary<ServerStatsInfo> _stats;
        private readonly PersistentDictionary<InternalServerStats> _internalStats;
        private readonly List<RecentMatchesItem> _recentMatches;
        private readonly List<PopularServersItem> _popularServers;

        private readonly ConcurrentDictionary<string, object> _locks = new ConcurrentDictionary<string, object>();

        public ServerStatistics(int maxReportSize)
        {
            _maxReportSize = maxReportSize;

            _servers = new PersistentDictionary<AdvertiseInfo>("Servers", "Advertise", false);
            _internalStats = new PersistentDictionary<InternalServerStats>("Servers", "InternalServerStats", false);
            _stats = new PersistentDictionary<ServerStatsInfo>("Servers", "ServerStats", false);
            _matches = new PersistentDictionary<MatchInfo>("Servers", "Match", true);
            _recentMatches =_matches
                .OrderByDescending(kv => kv.Key.Split(Path.DirectorySeparatorChar).Last().ToUtc())
                .Select(kv => new RecentMatchesItem
                {
                    Server = kv.Key.Split(Path.DirectorySeparatorChar).First(),
                    Timestamp = kv.Key.Split(Path.DirectorySeparatorChar).Last(),
                    Results = kv.Value
                })
                .Take(maxReportSize)
                .ToList();
            _popularServers = _stats
                .OrderByDescending(kv => kv.Value.AverageMatchesPerDay)
                .Select(kv => new PopularServersItem
                {
                    Endpoint = kv.Key,
                    Name = _servers[kv.Key].Name,
                    AverageMatchesPerDay = kv.Value.AverageMatchesPerDay
                })
                .Take(maxReportSize)
                .ToList();
            Console.WriteLine("rdy");
        }

        public void PutAdvertise(string endpoint, AdvertiseInfo info)
        {
            lock (_locks.GetOrAdd(endpoint, _ => new object()))
            {
                _servers[endpoint] = info;
            }
        }

        public bool HasAdvertise(string endpoint)
        {
            return _servers.ContainsKey(endpoint);
        }

        public AdvertiseInfo GetAdvertise(string endpoint)
        {
            return _servers[endpoint];
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
            var key = DoubleKey.Of(endpoint, timestamp);
            lock (_locks.GetOrAdd(endpoint, _ => new object()))
            {
                _matches[key] = info;
                CalcStats(endpoint, timestamp, info);
            }
        }

        public MatchInfo GetMatch(string endpoint, string timestamp)
        {
            return _matches[DoubleKey.Of(endpoint, timestamp)];
        }

        public ServerStatsInfo GetStats(string endpoint)
        {
            return _stats[endpoint];
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
            var newStats = oldStats.CalcNew(info, internalStats);

            _internalStats[endpoint] = internalStats;

            UpdateRecentMatchesReport(endpoint, timestamp, info);
            UpdatePopularServersReport(serverName, endpoint, newStats);

            _stats[endpoint] = newStats;
        }

        private void UpdateRecentMatchesReport(string endpoint, string timestamp, MatchInfo info)
        {
            lock (_recentMatches)
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
        }

        private void UpdatePopularServersReport(string serverName, string endpoint, ServerStatsInfo info)
        {
            lock (_popularServers)
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
}