using System;
using System.Collections.Generic;

namespace Kontur.GameStats.Server.Data
{
    public class InternalPlayerStats
    {
        public double TotalScoreboard { get; set; }
        public int TotalKills { get; set; }
        public int TotalDeaths { get; set; }
        public Dictionary<DateTime, int> MatchesPerDay { get; set; } = new Dictionary<DateTime, int>();
        public Dictionary<string, int> ServerFrequency { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> GameModeFrequency { get; set; } = new Dictionary<string, int>();
    }
}