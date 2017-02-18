using System;
using System.Collections.Generic;

namespace Kontur.GameStats.Server.Data
{
    [Serializable]
    public class MatchInfo
    {
        public string Map { get; set; }
        public string GameMode { get; set; }
        public int? FragLimit { get; set; }
        public int? TimeLimit { get; set; }
        public double? TimeElapsed { get; set; }
        public List<PlayerMatchInfo> Scoreboard { get; set; }

        public bool IsNotFull()
        {
            return Map == null || GameMode == null || FragLimit == null ||
                TimeLimit == null || TimeElapsed == null || Scoreboard == null;
        }
    }
}