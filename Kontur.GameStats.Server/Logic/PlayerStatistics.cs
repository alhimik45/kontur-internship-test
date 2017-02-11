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


            var totalPlayers = matchInfo.Scoreboard.Count;
            var playersBelowCurrent = totalPlayers - place;
            var scoreboardPercent = (double)playersBelowCurrent / (totalPlayers - 1) * 100;

            internalStats.TotalScoreboard += scoreboardPercent;
            internalStats.TotalKills += info.Kills;
            internalStats.TotalDeaths += info.Deaths;

            var currentUsesCount = internalStats.ServerFrequency.Get(endpoint) + 1;
            internalStats.ServerFrequency[endpoint] = currentUsesCount;
            string favoriteServer;
            if (oldStats.FavoriteServer == null)
            {
                favoriteServer = endpoint;
            }
            else
            {
                var favoriveUsesCount = internalStats.ServerFrequency[oldStats.FavoriteServer];
                favoriteServer = currentUsesCount > favoriveUsesCount ? endpoint : oldStats.FavoriteServer;
            }

            var currentModeMatchesCount = internalStats.GameModeFrequency.Get(matchInfo.GameMode) + 1;
            internalStats.GameModeFrequency[matchInfo.GameMode] = currentModeMatchesCount;
            string favoriteGameMode;
            if (oldStats.FavoriteGameMode == null)
            {
                favoriteGameMode = matchInfo.GameMode;
            }
            else
            {
                var favoriteModeMatchesCount = internalStats.GameModeFrequency[oldStats.FavoriteGameMode];
                favoriteGameMode = currentModeMatchesCount > favoriteModeMatchesCount
                    ? matchInfo.GameMode
                    : oldStats.FavoriteGameMode;
            }

            var time = timestamp.ToUtc();
            string lastTime;
            if (oldStats.LastMatchPlayed != null)
            {
                var oldTime = oldStats.LastMatchPlayed.ToUtc();
                lastTime = time > oldTime ? timestamp : oldStats.LastMatchPlayed;
            }
            else
            {
                lastTime = timestamp;
            }

            var currentDay = time.Date;
            var currentMatchesCount = internalStats.MatchesPerDay.Get(currentDay) + 1;
            internalStats.MatchesPerDay[currentDay] = currentMatchesCount;

            var totalMatches = oldStats.TotalMatchesPlayed + 1;
            var win = place == 1;
            var newStats = new PlayerStatsInfo
            {
                TotalMatchesPlayed = totalMatches,
                TotalMatchesWon = oldStats.TotalMatchesWon + (win ? 1 : 0),
                FavoriteServer = favoriteServer,
                UniqueServers = internalStats.ServerFrequency.Count,
                FavoriteGameMode = favoriteGameMode,
                AverageScoreboardPercent = internalStats.TotalScoreboard / totalMatches,
                MaximumMatchesPerDay = Math.Max(oldStats.MaximumMatchesPerDay, currentMatchesCount),
                AverageMatchesPerDay = (double)totalMatches / internalStats.MatchesPerDay.Count,
                LastMatchPlayed = lastTime,
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