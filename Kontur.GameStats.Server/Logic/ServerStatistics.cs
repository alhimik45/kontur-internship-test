using System;
using System.Collections.Generic;
using System.Linq;
using Kontur.GameStats.Server.Data;
using Kontur.GameStats.Server.Extensions;

namespace Kontur.GameStats.Server.Logic
{
    public class ServerStatistics
    {
        private readonly int _maxReportSize;

        private readonly Dictionary<string, AdvertiseInfo> _servers = new Dictionary<string, AdvertiseInfo>();
        private readonly Dictionary<string, Dictionary<string, MatchInfo>> _matches = new Dictionary<string, Dictionary<string, MatchInfo>>();
        private readonly Dictionary<string, ServerStatsInfo> _stats = new Dictionary<string, ServerStatsInfo>();
        private readonly Dictionary<string, InternalServerStats> _internalStats = new Dictionary<string, InternalServerStats>();
        private readonly List<RecentMatchesItem> _recentMatches = new List<RecentMatchesItem>();
        private readonly List<PopularServersItem> _popularServers = new List<PopularServersItem>();

        public ServerStatistics(int maxReportSize)
        {
            _maxReportSize = maxReportSize;
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
            Dictionary<string, MatchInfo> tmp;
            if (!_matches.TryGetValue(endpoint, out tmp))
            {
                tmp = _matches[endpoint] = new Dictionary<string, MatchInfo>();
            }
            CalcStats(endpoint, timestamp, info);
            tmp[timestamp] = info;
        }

        public MatchInfo GetMatch(string endpoint, string timestamp)
        {
            Dictionary<string, MatchInfo> tmp;
            _matches.TryGetValue(endpoint, out tmp);
            return tmp?.Get(timestamp);
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
        }
    }
}