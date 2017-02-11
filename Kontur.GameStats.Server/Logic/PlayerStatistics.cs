using System;
using System.Collections.Generic;
using System.Linq;
using Kontur.GameStats.Server.Data;
using Kontur.GameStats.Server.Extensions;

namespace Kontur.GameStats.Server.Logic
{
    public class PlayerStatistics
    {
        private readonly int _maxReportSize;

        private readonly Dictionary<string, PlayerStatsInfo> _stats = new Dictionary<string, PlayerStatsInfo>();
        private readonly Dictionary<string, InternalPlayerStats> _internalStats = new Dictionary<string, InternalPlayerStats>();
        private readonly List<BestPlayersItem> _bestPlayers = new List<BestPlayersItem>();

        public PlayerStatistics(int maxReportSize)
        {
            _maxReportSize = maxReportSize;
        }

        private static string UpdateFavorite(IDictionary<string, int> frequency, string updatedValue, string oldValue)
        {
            var currentUsesCount = frequency.Get(updatedValue) + 1;
            frequency[updatedValue] = currentUsesCount;
            if (oldValue == null)
            {
                return updatedValue;
            }
            var favoriveUsesCount = frequency[oldValue];
            return currentUsesCount > favoriveUsesCount ? updatedValue : oldValue;
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
            return _stats.Get(name.ToLower());
        }

        public List<BestPlayersItem> GetBestPlayers(int count)
        {
            return _bestPlayers.Take(count).ToList();
        }

        private void CalcPlayerStats(string endpoint, string timestamp, int index, MatchInfo matchInfo)
        {
            var info = matchInfo.Scoreboard[index];
            var place = index + 1;
            var playerName = info.Name.ToLower();
            var time = timestamp.ToUtc();
            var currentDay = time.Date;
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

            internalStats.Update(currentDay, place, matchInfo, info);

            var totalMatches = oldStats.TotalMatchesPlayed + 1;
            var newStats = new PlayerStatsInfo
            {
                TotalMatchesPlayed = totalMatches,
                LastMatchPlayed = oldStats.GetLastTimePlayed(timestamp),
                UniqueServers = internalStats.ServerFrequency.Count,
                TotalMatchesWon = oldStats.TotalMatchesWon + (place == 1 ? 1 : 0),
                FavoriteServer = UpdateFavorite(internalStats.ServerFrequency, endpoint, oldStats.FavoriteServer),
                FavoriteGameMode = UpdateFavorite(internalStats.GameModeFrequency, matchInfo.GameMode, oldStats.FavoriteGameMode),
                AverageScoreboardPercent = internalStats.TotalScoreboard / totalMatches,
                MaximumMatchesPerDay = Math.Max(oldStats.MaximumMatchesPerDay, internalStats.MatchesPerDay[currentDay]),
                AverageMatchesPerDay = (double)totalMatches / internalStats.MatchesPerDay.Count,
                KillToDeathRatio = (double)internalStats.TotalKills / internalStats.TotalDeaths
            };

            if (internalStats.TotalDeaths != 0 && newStats.TotalMatchesPlayed >= 10)
            {
                UpdateBestPlayersReport(playerName, newStats);
            }

            _stats[playerName] = newStats;
        }

        private void UpdateBestPlayersReport(string name, PlayerStatsInfo info)
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