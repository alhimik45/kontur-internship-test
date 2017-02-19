using System;
using System.Collections.Generic;
using Kontur.GameStats.Server.Extensions;

namespace Kontur.GameStats.Server.Data
{
    [Serializable]
    public class PublicPlayerStats
    {
        public int TotalMatchesPlayed { get; set; }
        public int TotalMatchesWon { get; set; }
        public string FavoriteServer { get; set; }
        public int UniqueServers { get; set; }
        public string FavoriteGameMode { get; set; }
        public double AverageScoreboardPercent { get; set; }
        public int MaximumMatchesPerDay { get; set; }
        public double AverageMatchesPerDay { get; set; }
        public string LastMatchPlayed { get; set; }
        public double KillToDeathRatio { get; set; }

        public PublicPlayerStats CalcNew(string endpoint, string timestamp, int place, PlayerStats stats, MatchInfo matchInfo, PlayerMatchInfo info)
        {
            var time = timestamp.ToUtc();
            var totalMatches = TotalMatchesPlayed + 1;
            var favoriteServer = UpdateFavorite(stats.ServerFrequency, endpoint, FavoriteServer);
            var favoriteGameMode = UpdateFavorite(stats.GameModeFrequency, matchInfo.GameMode, FavoriteGameMode);
            return new PublicPlayerStats
            {
                TotalMatchesPlayed = totalMatches,
                LastMatchPlayed = GetLastTimePlayed(timestamp),
                FavoriteServer = favoriteServer,
                FavoriteGameMode = favoriteGameMode,
                UniqueServers = stats.ServerFrequency.Count,
                TotalMatchesWon = TotalMatchesWon + (place == 1 ? 1 : 0),
                AverageScoreboardPercent = stats.TotalScoreboard / totalMatches,
                MaximumMatchesPerDay = Math.Max(MaximumMatchesPerDay, stats.MatchesPerDay[time.Date]),
                AverageMatchesPerDay = (double)totalMatches / stats.MatchesPerDay.Count,
                KillToDeathRatio = (double)stats.TotalKills / stats.TotalDeaths
            };
        }

        private string GetLastTimePlayed(string timestamp)
        {
            if (LastMatchPlayed != null)
            {
                return timestamp.ToUtc() > LastMatchPlayed.ToUtc() ? timestamp : LastMatchPlayed;
            }
            return timestamp;
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
    }
}