using System;

namespace Kontur.GameStats.Server.Data
{
    /// <summary>
    /// Класс элемента списка недавних матчей
    /// </summary>
    [Serializable]
    public class RecentMatchesItem
    {
        public string Server { get; set; }
        public string Timestamp { get; set; }
        public MatchInfo Results { get; set; }
    }
}