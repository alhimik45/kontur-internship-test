using System;
using System.Collections.Generic;
using Kontur.GameStats.Server.Extensions;

namespace Kontur.GameStats.Server.Data
{
    public class InternalPlayerStats
    {
        public double TotalScoreboard { get; set; }
        public int TotalKills { get; set; }
        public int TotalDeaths { get; set; }
        public Dictionary<DateTime, int> MatchesPerDay { get; } = new Dictionary<DateTime, int>();
        public Dictionary<string, int> ServerFrequency { get; } = new Dictionary<string, int>();
        public Dictionary<string, int> GameModeFrequency { get; } = new Dictionary<string, int>();

        public void Update(DateTime currentDay, int place, MatchInfo matchInfo, PlayerMatchInfo info)
        {
            var totalPlayers = matchInfo.Scoreboard.Count;
            var playersBelowCurrent = totalPlayers - place;
            var scoreboardPercent = (double)playersBelowCurrent / (totalPlayers - 1) * 100;
            MatchesPerDay[currentDay] = MatchesPerDay.Get(currentDay) + 1;
            TotalScoreboard += scoreboardPercent;
            TotalKills += info.Kills;
            TotalDeaths += info.Deaths;
        }
    }
}