using System;
using System.Collections.Concurrent;
using Kontur.GameStats.Server.Util;
// ReSharper disable PossibleInvalidOperationException

namespace Kontur.GameStats.Server.Data
{
    [Serializable]
    public class PlayerStats
    {
        public double TotalScoreboard { get; set; }
        public int TotalKills { get; set; }
        public int TotalDeaths { get; set; }
        public PublicPlayerStats PublicStats { get; set; }
        public ConcurrentDictionary<DateTime, int> MatchesPerDay { get; set; } = new ConcurrentDictionary<DateTime, int>();
        public ConcurrentDictionary<string, int> ServerFrequency { get; set; } = new ConcurrentDictionary<string, int>();
        public ConcurrentDictionary<string, int> GameModeFrequency { get; set; } = new ConcurrentDictionary<string, int>();

        public PlayerStats CalcNew(DateTime time, int place, MatchInfo matchInfo, PlayerMatchInfo info)
        {
            var totalPlayers = matchInfo.Scoreboard.Count;
            var playersBelowCurrent = totalPlayers - place;
            double scoreboardPercent;
            if (totalPlayers == 1)
            {
                scoreboardPercent = 100;
            }
            else
            {
                scoreboardPercent = (double)playersBelowCurrent / (totalPlayers - 1) * 100;
            }
            MatchesPerDay[time.Date] = MatchesPerDay.Get(time.Date) + 1;
            return new PlayerStats
            {
                TotalDeaths = TotalDeaths + info.Deaths.Value,
                TotalKills = TotalKills + info.Kills.Value,
                TotalScoreboard = TotalScoreboard + scoreboardPercent,
                MatchesPerDay = MatchesPerDay,
                ServerFrequency = ServerFrequency,
                GameModeFrequency = GameModeFrequency
            };
        }
    }
}