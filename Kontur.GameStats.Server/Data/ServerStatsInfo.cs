using System;
using System.Collections.Generic;
using System.Linq;
using Kontur.GameStats.Server.Extensions;

namespace Kontur.GameStats.Server.Data
{
    [Serializable]
    public class ServerStatsInfo
    {
        public int TotalMatchesPlayed { get; set; }
        public int MaximumMatchesPerDay { get; set; }
        public double AverageMatchesPerDay { get; set; }
        public int MaximumPopulation { get; set; }
        public double AveragePopulation { get; set; }
        public List<string> Top5GameModes { get; set; } = new List<string>();
        public List<string> Top5Maps { get; set; } = new List<string>();

        public ServerStatsInfo CalcNew(MatchInfo info, InternalServerStats internalStats)
        {
            var totalMatches = TotalMatchesPlayed + 1;
            return new ServerStatsInfo
            {
                TotalMatchesPlayed = totalMatches,
                Top5Maps = GetTop5Maps(info, internalStats),
                Top5GameModes = GetTop5Modes(info, internalStats),
                MaximumMatchesPerDay = Math.Max(MaximumMatchesPerDay, internalStats.MatchesInLastDay),
                AverageMatchesPerDay = (double)totalMatches / internalStats.DaysWithMatchesCount,
                MaximumPopulation = Math.Max(MaximumPopulation, info.Scoreboard.Count),
                AveragePopulation = (double)internalStats.TotalPopulation / totalMatches
            };
        }

        private List<string> GetTop5Maps(MatchInfo info, InternalServerStats internalStats)
        {
            return Top5Maps.ToList().UpdateTop(5,
                m => internalStats.MapFrequency[m],
                m => m,
                info.Map).ToList();
        }

        private List<string> GetTop5Modes(MatchInfo info, InternalServerStats internalStats)
        {
            return Top5GameModes.ToList().UpdateTop(5,
                m => internalStats.GameModeFrequency[m],
                m => m,
                info.GameMode).ToList();
        }
    }
}