using System;
using System.Collections.Concurrent;
using Kontur.GameStats.Server.Extensions;

namespace Kontur.GameStats.Server.Data
{
    [Serializable]
    public class InternalServerStats
    {
        public DateTime LastMatchDay { get; set; }
        public int MatchesInLastDay { get; set; }
        public int TotalPopulation { get; set; }
        public int DaysWithMatchesCount { get; set; } = 1;
        public ConcurrentDictionary<string, int> MapFrequency { get; } = new ConcurrentDictionary<string, int>();
        public ConcurrentDictionary<string, int> GameModeFrequency { get; } = new ConcurrentDictionary<string, int>();

        public void Update(DateTime time, MatchInfo info)
        {
            if (time.Date == LastMatchDay)
            {
                MatchesInLastDay += 1;
            }
            else
            {
                DaysWithMatchesCount += 1;
                LastMatchDay = time.Date;
                MatchesInLastDay = 1;
            }
            TotalPopulation += info.Scoreboard.Count;
            GameModeFrequency[info.GameMode] = GameModeFrequency.Get(info.GameMode) + 1;
            MapFrequency[info.Map] = MapFrequency.Get(info.Map) + 1;
        }
    }
}