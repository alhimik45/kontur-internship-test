namespace Kontur.GameStats.Server.Data
{
    public class RecentMatchesItem
    {
        public string Server { get; set; }
        public string Timestamp { get; set; }
        public MatchInfo Results { get; set; }
    }
}