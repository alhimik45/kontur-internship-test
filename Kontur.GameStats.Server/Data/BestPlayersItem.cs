using System;

namespace Kontur.GameStats.Server.Data
{
    [Serializable]
    public class BestPlayersItem
    {
        public string Name { get; set; }
        public double KillToDeathRatio { get; set; }
    }
}