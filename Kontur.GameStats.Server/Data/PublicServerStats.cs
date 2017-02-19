using System;
using System.Collections.Generic;
using System.Linq;
using Kontur.GameStats.Server.Util;

namespace Kontur.GameStats.Server.Data
{
    [Serializable]
    public class PublicServerStats
    {
        public int TotalMatchesPlayed { get; set; }
        public int MaximumMatchesPerDay { get; set; }
        public double AverageMatchesPerDay { get; set; }
        public int MaximumPopulation { get; set; }
        public double AveragePopulation { get; set; }
        public List<string> Top5GameModes { get; set; } = new List<string>();
        public List<string> Top5Maps { get; set; } = new List<string>();

        public PublicServerStats CalcNew(MatchInfo info, ServerStats internalStats)
        {
            return new PublicServerStats
            {
                TotalMatchesPlayed = TotalMatchesPlayed + 1,
                Top5Maps = GetTop5Maps(info, internalStats),
                Top5GameModes = GetTop5Modes(info, internalStats),
                MaximumMatchesPerDay = Math.Max(MaximumMatchesPerDay, internalStats.MatchesInLastDay),
                AverageMatchesPerDay = (double)(TotalMatchesPlayed + 1) / internalStats.DaysWithMatches,
                MaximumPopulation = Math.Max(MaximumPopulation, info.Scoreboard.Count),
                AveragePopulation = (double)internalStats.TotalPopulation / (TotalMatchesPlayed + 1)
            };
        }

        private List<string> GetTop5Maps(MatchInfo info, ServerStats internalStats)
        {
            return Top5Maps.ToList().UpdateTop(5,
                m => internalStats.MapFrequency[m],
                m => m,
                info.Map).ToList();
        }

        private List<string> GetTop5Modes(MatchInfo info, ServerStats internalStats)
        {
            return Top5GameModes.ToList().UpdateTop(5,
                m => internalStats.GameModeFrequency[m],
                m => m,
                info.GameMode).ToList();
        }
    }
}