using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kontur.GameStats.Server.Data;
using Kontur.GameStats.Server.Util;

namespace Kontur.GameStats.Server.Logic
{
    public class ServerStatistics
    {
        private const string PopularServersFilename = "Reports/PopularServers";
        private const string RecentMatchesFilename = "Reports/RecentMatches";
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

            _servers = new PersistentDictionary<AdvertiseInfo>("Servers", "Advertise", memoryMirror: true);
            _stats = new PersistentDictionary<ServerStats>("Servers", "ServerStats", memoryMirror: true);
            _matches = new PersistentDictionary<MatchInfo>("Servers", "Match", doubleKey: true);
            Directory.CreateDirectory("Reports");
            _recentMatches = Collections.Load<RecentMatchesItem>(RecentMatchesFilename);
            _popularServers = Collections.Load<PopularServersItem>(PopularServersFilename);
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
            return _servers.Select(kv => new ServersInfoItem
            {
                Endpoint = kv.Key,
                Info = kv.Value
            });
        }

        public bool PutMatch(string endpoint, string timestamp, MatchInfo info)
        {
            var lowerEndpoint = endpoint.ToLower();
            lock (_locks.GetOrAdd(lowerEndpoint, _ => new object()))
            {
                if (_matches[lowerEndpoint, timestamp] != null)
                {
                    return false;
                }
                _matches[lowerEndpoint, timestamp] = info;
                CalcStats(lowerEndpoint, timestamp, info);
                return true;
            }
        }

        public MatchInfo GetMatch(string endpoint, string timestamp)
        {
            var lowerEndpoint = endpoint.ToLower();
            lock (_locks.GetOrAdd(lowerEndpoint, _ => new object()))
            {
                return _matches[lowerEndpoint, timestamp];
            }
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
                Collections.Save(_recentMatches, RecentMatchesFilename);
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
                Collections.Save(_popularServers, PopularServersFilename);
            }
        }
    }
}