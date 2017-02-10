using System;
using System.Collections.Generic;
using System.Linq;
using Kontur.GameStats.Server.Data;
using Kontur.GameStats.Server.Extensions;

namespace Kontur.GameStats.Server
{
    public class ServerStatistics
    {
        private int _maxReportSize;

        private Dictionary<string, AdvertiseInfo> _servers = new Dictionary<string, AdvertiseInfo>();
        private Dictionary<string, Dictionary<string, MatchInfo>> _matches = new Dictionary<string, Dictionary<string, MatchInfo>>();
        private Dictionary<string, ServerStatsInfo> _stats = new Dictionary<string, ServerStatsInfo>();
        private Dictionary<string, InternalServerStats> _internalStats = new Dictionary<string, InternalServerStats>();
        private List<RecentMatchesItem> _recentMatches = new List<RecentMatchesItem>();
        private List<PopularServersItem> _popularServers = new List<PopularServersItem>();


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
            AdvertiseInfo info;
            _servers.TryGetValue(endpoint, out info);
            return info;
        }

        public List<ServersInfoItem> GetAll()
        {
            return _servers.Select(kv => new ServersInfoItem
            {
                Endpoint = kv.Key,
                Info = kv.Value
            }).ToList();
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
            if (!_matches.TryGetValue(endpoint, out tmp)) return null;
            MatchInfo info;
            tmp.TryGetValue(timestamp, out info);
            return info;
        }

        public ServerStatsInfo GetStats(string endpoint)
        {
            ServerStatsInfo stats;
            _stats.TryGetValue(endpoint, out stats);
            return stats;
        }

        private void CalcStats(string endpoint, string timestamp, MatchInfo info)
        {
            ServerStatsInfo oldStats;
            InternalServerStats internalStats;
            var time = timestamp.ToUtc();
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

            if (time.Date == internalStats.LastMatchDay)
            {
                internalStats.MatchesInLastDay += 1;
            }
            else
            {
                internalStats.DaysWithMatchesCount += 1;
                internalStats.LastMatchDay = time.Date;
                internalStats.MatchesInLastDay = 1;
            }
            internalStats.TotalPopulation += info.Scoreboard.Count;
            if (internalStats.GameModeFrequency.ContainsKey(info.GameMode))//TODO
            {
                internalStats.GameModeFrequency[info.GameMode] += 1;
            }
            else
            {
                internalStats.GameModeFrequency[info.GameMode] = 1;
            }
            if (internalStats.MapFrequency.ContainsKey(info.Map))//TODO
            {
                internalStats.MapFrequency[info.Map] += 1;
            }
            else
            {
                internalStats.MapFrequency[info.Map] = 1;
            }

            var totalMatches = oldStats.TotalMatchesPlayed + 1;

            var newStats = new ServerStatsInfo
            {
                Name = _servers[endpoint].Name,
                TotalMatchesPlayed = totalMatches,
                MaximumMatchesPerDay = Math.Max(oldStats.MaximumMatchesPerDay, internalStats.MatchesInLastDay),
                AverageMatchesPerDay = (double)totalMatches / internalStats.DaysWithMatchesCount,
                MaximumPopulation = Math.Max(oldStats.MaximumPopulation, info.Scoreboard.Count),
                AveragePopulation = (double)internalStats.TotalPopulation / totalMatches,
                Top5Maps = internalStats.MapFrequency
                    .OrderByDescending(kv => kv.Value)
                    .Take(5)
                    .Select(kv => kv.Key)
                    .ToList(),
                Top5GameModes = internalStats.GameModeFrequency
                    .OrderByDescending(kv => kv.Value)
                    .Take(5)
                    .Select(kv => kv.Key)
                    .ToList()
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

        public List<RecentMatchesItem> GetRecentMatches(int count)
        {
            return _recentMatches.Take(count).ToList();
        }

        public List<PopularServersItem> GetPopularServers(int count)
        {
            return _popularServers.Take(count).ToList();
        }
    }
}