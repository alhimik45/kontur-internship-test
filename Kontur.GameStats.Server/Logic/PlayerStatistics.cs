using System.Collections.Generic;
using System.Linq;
using Kontur.GameStats.Server.Data;
using Kontur.GameStats.Server.Extensions;
using Kontur.GameStats.Server.Util;
using LiteDB;

namespace Kontur.GameStats.Server.Logic
{
    public class PlayerStatistics
    {
        private readonly LiteDatabase _db;
        private readonly int _maxReportSize;

        private readonly PersistentDictionary<string, PlayerStatsInfo> _stats;
        private readonly PersistentDictionary<string, InternalPlayerStats> _internalStats;
        private readonly PersistentList<BestPlayersItem> _bestPlayers;

        public PlayerStatistics(LiteDatabase db, int maxReportSize)
        {
            _db = db;
            _maxReportSize = maxReportSize;

            _stats = new PersistentDictionary<string, PlayerStatsInfo>(db, "PlayerStats");
            _internalStats = new PersistentDictionary<string, InternalPlayerStats>(db, "InternalPlayerStats");
            _bestPlayers = new PersistentList<BestPlayersItem>(db, "BestPlayers");
        }

        public void AddMatchInfo(string endpoint, string timestamp, MatchInfo info)
        {
            for (var i = 0; i < info.Scoreboard.Count; ++i)
            {
                CalcPlayerStats(endpoint, timestamp, i, info);
            }
        }

        public PlayerStatsInfo GetStats(string name)
        {
            return _stats.Get(name.ToLower());
        }

        public List<BestPlayersItem> GetBestPlayers(int count)
        {
            return _bestPlayers.Take(count).ToList();
        }

        private void CalcPlayerStats(string endpoint, string timestamp, int index, MatchInfo matchInfo)
        {
            var info = matchInfo.Scoreboard[index];
            var place = index + 1;
            var playerName = info.Name.ToLower();
            PlayerStatsInfo oldStats;
            InternalPlayerStats internalStats;
            if (!_stats.TryGetValue(playerName, out oldStats))
            {
                internalStats = new InternalPlayerStats();
                _internalStats[playerName] = internalStats;
                oldStats = new PlayerStatsInfo();
            }
            else
            {
                internalStats = _internalStats[playerName];
            }

            internalStats.Update(timestamp.ToUtc(), place, matchInfo, info);

            var newStats = oldStats.CalcNew(endpoint, timestamp, place, internalStats, matchInfo, info);

            lock (this)
            {
                using (var transaction = _db.BeginTrans())
                {
                    if (internalStats.TotalDeaths != 0 && newStats.TotalMatchesPlayed >= 10)
                    {
                        UpdateBestPlayersReport(playerName, newStats);
                    }
                    _internalStats[playerName] = internalStats;
                    _stats[playerName] = newStats;
                    transaction.Commit();
                }
            }
        }

        private void UpdateBestPlayersReport(string name, PlayerStatsInfo info)
        {
            _bestPlayers.UpdateTop(_maxReportSize,
                bp => bp.KillToDeathRatio,
                bp => bp.Name,
                new BestPlayersItem
                {
                    Name = name,
                    KillToDeathRatio = info.KillToDeathRatio
                });
        }
    }
}