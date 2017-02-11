using Kontur.GameStats.Server.Extensions;

namespace Kontur.GameStats.Server.Data
{
    public class PlayerStatsInfo
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

        public string GetLastTimePlayed(string timestamp)
        {
            if (LastMatchPlayed != null)
            {
                return timestamp.ToUtc() > LastMatchPlayed.ToUtc() ? timestamp : LastMatchPlayed;
            }
            return timestamp;
        }
    }
}