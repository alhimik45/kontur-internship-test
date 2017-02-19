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
        private readonly PersistentDictionary<ServerStats> _stats;
        private readonly List<RecentMatchesItem> _recentMatches;
        private readonly List<PopularServersItem> _popularServers;

        private readonly ConcurrentDictionary<string, object> _locks = new ConcurrentDictionary<string, object>();

        public ServerStatistics(int maxReportSize)
        {
            _maxReportSize = maxReportSize;

            _servers = new PersistentDictionary<AdvertiseInfo>("Servers", "Advertise", false);
            _stats = new PersistentDictionary<ServerStats>("Servers", "ServerStats", false);
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
                .OrderByDescending(kv => kv.Value.PublicStats.AverageMatchesPerDay)
                .Select(kv => new PopularServersItem
                {
                    Endpoint = kv.Key,
                    Name = _servers[kv.Key].Name,
                    AverageMatchesPerDay = kv.Value.PublicStats.AverageMatchesPerDay
                })
                .Take(maxReportSize)
                .ToList();
            Console.WriteLine("rdy");
        }

        public void PutAdvertise(string endpoint, AdvertiseInfo info)
        {
            var lowerEndpoint = endpoint.ToLower();
            lock (_locks.GetOrAdd(lowerEndpoint, _ => new object()))
            {
                _servers[lowerEndpoint] = info;
            }
        }

        public bool HasAdvertise(string endpoint)
        {
            return _servers.ContainsKey(endpoint.ToLower());
        }

        public AdvertiseInfo GetAdvertise(string endpoint)
        {
            return _servers[endpoint.ToLower()];
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
            var lowerEndpoint = endpoint.ToLower();
            var key = DoubleKey.Of(lowerEndpoint, timestamp);
            lock (_locks.GetOrAdd(lowerEndpoint, _ => new object()))
            {
                _matches[key] = info;
                CalcStats(lowerEndpoint, timestamp, info);
            }
        }

        public MatchInfo GetMatch(string endpoint, string timestamp)
        {
            return _matches[DoubleKey.Of(endpoint.ToLower(), timestamp)];
        }

        public PublicServerStats GetStats(string endpoint)
        {
            return _stats[endpoint.ToLower()]?.PublicStats;
        }

        public List<RecentMatchesItem> GetRecentMatches(int count)
        {
            lock (_recentMatches)
            {
                return _recentMatches.Take(count).ToList();
            }
        }

        public List<PopularServersItem> GetPopularServers(int count)
        {
            lock (_popularServers)
            {
                return _popularServers.Take(count).ToList();
            }
        }

        private void CalcStats(string endpoint, string timestamp, MatchInfo info)
        {
            var time = timestamp.ToUtc();
            ServerStats oldStats;
            if (!_stats.TryGetValue(endpoint, out oldStats))
            {
                oldStats = new ServerStats
                {
                    LastMatchDay = time.Date,
                    PublicStats = new PublicServerStats()
                };
            }

            var newStats = oldStats.CalcNew(time, info);
            var serverName = _servers[endpoint].Name;
            newStats.PublicStats = oldStats.PublicStats.CalcNew(info, newStats);

            UpdateRecentMatchesReport(endpoint, timestamp, info);
            UpdatePopularServersReport(serverName, endpoint, newStats.PublicStats);

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

        private void UpdatePopularServersReport(string serverName, string endpoint, PublicServerStats info)
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