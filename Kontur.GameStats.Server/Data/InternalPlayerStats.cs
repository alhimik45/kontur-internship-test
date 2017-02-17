using System;
using System.Collections.Concurrent;
using Kontur.GameStats.Server.Extensions;

namespace Kontur.GameStats.Server.Data
{
    public class InternalPlayerStats
    {
        public double TotalScoreboard { get; set; }
        public int TotalKills { get; set; }
        public int TotalDeaths { get; set; }
        public ConcurrentDictionary<DateTime, int> MatchesPerDay { get; } = new ConcurrentDictionary<DateTime, int>();
        public ConcurrentDictionary<string, int> ServerFrequency { get; } = new ConcurrentDictionary<string, int>();
        public ConcurrentDictionary<string, int> GameModeFrequency { get; } = new ConcurrentDictionary<string, int>();

        public void Update(DateTime time, int place, MatchInfo matchInfo, PlayerMatchInfo info)
        {
            var totalPlayers = matchInfo.Scoreboard.Count;
            var playersBelowCurrent = totalPlayers - place;
            var scoreboardPercent = (double)playersBelowCurrent / (totalPlayers - 1) * 100;
            MatchesPerDay[time.Date] = MatchesPerDay.Get(time.Date) + 1;
            TotalScoreboard += scoreboardPercent;
            TotalKills += info.Kills;
            TotalDeaths += info.Deaths;
        }
    }
}