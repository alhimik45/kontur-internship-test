using System;
using System.Collections.Generic;

namespace Kontur.GameStats.Server.Data
{
    public class InternalServerStats
    {
        public DateTime LastMatchDay { get; set; }
        public int MatchesInLastDay { get; set; }
        public int TotalPopulation { get; set; }
        public int DaysWithMatchesCount { get; set; } = 1;
        public Dictionary<string, int> MapFrequency { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> GameModeFrequency { get; set; } = new Dictionary<string, int>();
    }
}