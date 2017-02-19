using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Kontur.GameStats.Server.Data;
using Kontur.GameStats.Server.Extensions;
using Kontur.GameStats.Server.Util;

namespace Kontur.GameStats.Server.Logic
{
    public class PlayerStatistics
    {
        private readonly int _maxReportSize;

        private readonly PersistentDictionary<PlayerStats> _stats;
        private readonly List<BestPlayersItem> _bestPlayers;

        private readonly ConcurrentDictionary<string, object> _locks = new ConcurrentDictionary<string, object>();

        public PlayerStatistics(int maxReportSize)
        {
            _maxReportSize = maxReportSize;

            _stats = new PersistentDictionary<PlayerStats>("Players", "PlayerStats", false);

            _bestPlayers = _stats
                .Where(kv => kv.Value.PublicStats.TotalMatchesPlayed >= 10 && _stats[kv.Key].TotalDeaths != 0)
                .OrderByDescending(kv => kv.Value.PublicStats.KillToDeathRatio)
                .Select(kv => new BestPlayersItem
                {
                    Name = kv.Key,
                    KillToDeathRatio = kv.Value.PublicStats.KillToDeathRatio
                })
                .Take(maxReportSize)
                .ToList();
            Console.WriteLine("rdy");
        }

        public void AddMatchInfo(string endpoint, string timestamp, MatchInfo info)
        {
            for (var i = 0; i < info.Scoreboard.Count; ++i)
            {
                CalcPlayerStats(endpoint, timestamp, i, info);
            }
        }

        public PublicPlayerStats GetStats(string name)
        {
            return _stats[name.ToLower()]?.PublicStats;
        }

        public List<BestPlayersItem> GetBestPlayers(int count)
        {
            lock (_bestPlayers)
            {
                return _bestPlayers.Take(count).ToList();
            }
        }

        private void CalcPlayerStats(string endpoint, string timestamp, int index, MatchInfo matchInfo)
        {
            var info = matchInfo.Scoreboard[index];
            var playerName = info.Name.ToLower();

            lock (_locks.GetOrAdd(playerName, _ => new object()))
            {
                var place = index + 1;
                PlayerStats oldStats;
                if (!_stats.TryGetValue(playerName, out oldStats))
                {
                    oldStats = new PlayerStats { PublicStats = new PublicPlayerStats() };
                }

                var newStats = oldStats.CalcNew(timestamp.ToUtc(), place, matchInfo, info);

                newStats.PublicStats = oldStats.PublicStats.CalcNew(endpoint, timestamp, place, newStats, matchInfo, info);

                if (newStats.TotalDeaths != 0 && newStats.PublicStats.TotalMatchesPlayed >= 10)
                {
                    UpdateBestPlayersReport(playerName, newStats.PublicStats);
                }
                _stats[playerName] = newStats;
            }
        }

        private void UpdateBestPlayersReport(string name, PublicPlayerStats info)
        {
            lock (_bestPlayers)
            {
                _bestPlayers.UpdateTop(_maxReportSize,
                    bp => bp.KillToDeathRatio,
                    bp => bp.Name,
                    new BestPlayersItem
                    {
                        Name = name,
                        KillToDeathRatio = info.KillToDeathRatio
                    });
            }
        }
    }
}