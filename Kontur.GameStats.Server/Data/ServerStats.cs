using System;
using System.Collections.Concurrent;
using Kontur.GameStats.Server.Util;

namespace Kontur.GameStats.Server.Data
{
    [Serializable]
    public class ServerStats
    {
        public DateTime LastMatchDay { get; set; }
        public int MatchesInLastDay { get; set; }
        public int TotalPopulation { get; set; }
        public PublicServerStats PublicStats { get; set; }
        public int DaysWithMatches { get; set; } = 1;
        public ConcurrentDictionary<string, int> MapFrequency { get; set; } = new ConcurrentDictionary<string, int>();
        public ConcurrentDictionary<string, int> GameModeFrequency { get; set; } = new ConcurrentDictionary<string, int>();

        public ServerStats CalcNew(DateTime time, MatchInfo info)
        {
            var matchesInLastDay = MatchesInLastDay;
            var daysWithMatches = DaysWithMatches;
            var lastMatchDay = LastMatchDay;

            if (time.Date == LastMatchDay)
            {
                matchesInLastDay += 1;
            }
            else
            {
                daysWithMatches += 1;
                lastMatchDay = time.Date;
                matchesInLastDay = 1;
            }
            GameModeFrequency[info.GameMode] = GameModeFrequency.Get(info.GameMode) + 1;
            MapFrequency[info.Map] = MapFrequency.Get(info.Map) + 1;
            return new ServerStats
            {
                TotalPopulation = TotalPopulation + info.Scoreboard.Count,
                DaysWithMatches = daysWithMatches,
                LastMatchDay = lastMatchDay,
                MatchesInLastDay = matchesInLastDay,
                MapFrequency = MapFrequency,
                GameModeFrequency = GameModeFrequency
            };
        }
    }
}