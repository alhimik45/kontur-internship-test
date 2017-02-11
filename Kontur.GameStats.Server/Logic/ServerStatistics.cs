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
            return _matches.TryGetValue(endpoint, out tmp) ? tmp.Get(timestamp) : null;
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

            var top5Modes = oldStats.Top5GameModes.ToList().UpdateTop(5,
                m => internalStats.GameModeFrequency[m],
                m => m,
                info.GameMode);
            var top5Maps = oldStats.Top5Maps.ToList().UpdateTop(5,
                m => internalStats.MapFrequency[m],
                m => m,
                info.Map);

            var totalMatches = oldStats.TotalMatchesPlayed + 1;
            var newStats = new ServerStatsInfo
            {
                TotalMatchesPlayed = totalMatches,
                Top5Maps = top5Maps,
                Top5GameModes = top5Modes,
                Name = _servers[endpoint].Name,
                MaximumMatchesPerDay = Math.Max(oldStats.MaximumMatchesPerDay, internalStats.MatchesInLastDay),
                AverageMatchesPerDay = (double)totalMatches / internalStats.DaysWithMatchesCount,
                MaximumPopulation = Math.Max(oldStats.MaximumPopulation, info.Scoreboard.Count),
                AveragePopulation = (double)internalStats.TotalPopulation / totalMatches
            };

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