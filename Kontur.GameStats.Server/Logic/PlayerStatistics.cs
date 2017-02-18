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

        private readonly PersistentDictionary<PlayerStatsInfo> _stats;
        private readonly PersistentDictionary<InternalPlayerStats> _internalStats;
        private readonly List<BestPlayersItem> _bestPlayers;

        private readonly ConcurrentDictionary<string, object> _locks = new ConcurrentDictionary<string, object>();

        public PlayerStatistics(int maxReportSize)
        {
            _maxReportSize = maxReportSize;

            _stats = new PersistentDictionary<PlayerStatsInfo>("Players", "PlayerStats", false);
            _internalStats = new PersistentDictionary<InternalPlayerStats>("Players", "InternalPlayerStats", false);
            _bestPlayers = _stats
                .Where(kv => kv.Value.TotalMatchesPlayed >= 10 && _internalStats[kv.Key].TotalDeaths != 0)
                .OrderByDescending(kv => kv.Value.KillToDeathRatio)
                .Select(kv => new BestPlayersItem
                {
                    Name = kv.Key,
                    KillToDeathRatio = kv.Value.KillToDeathRatio
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

        public PlayerStatsInfo GetStats(string name)
        {
            return _stats[name.ToLower()];
        }

        public List<BestPlayersItem> GetBestPlayers(int count)
        {
            return _bestPlayers.Take(count).ToList();
        }

        private void CalcPlayerStats(string endpoint, string timestamp, int index, MatchInfo matchInfo)
        {
            var info = matchInfo.Scoreboard[index];
            var playerName = info.Name.ToLower();

            lock (_locks.GetOrAdd(playerName, _ => new object()))
            {
                var place = index + 1;
                PlayerStatsInfo oldStats;
                InternalPlayerStats internalStats;
                if (!_stats.TryGetValue(playerName, out oldStats))
                {
                    internalStats = new InternalPlayerStats();
                    _internalStats[playerName] = internalStats;
                    oldStats = new PlayerStatsInfo();
                }
                else
                {
                    internalStats = _internalStats[playerName];
                }

                internalStats.Update(timestamp.ToUtc(), place, matchInfo, info);

                var newStats = oldStats.CalcNew(endpoint, timestamp, place, internalStats, matchInfo, info);

                if (internalStats.TotalDeaths != 0 && newStats.TotalMatchesPlayed >= 10)
                {
                    UpdateBestPlayersReport(playerName, newStats);
                }
                _internalStats[playerName] = internalStats;
                _stats[playerName] = newStats;
            }
        }

        private void UpdateBestPlayersReport(string name, PlayerStatsInfo info)
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