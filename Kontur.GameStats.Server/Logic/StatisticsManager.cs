using System.Collections.Generic;
using Kontur.GameStats.Server.Data;
using Kontur.GameStats.Server.Extensions;
using LiteDB;

namespace Kontur.GameStats.Server.Logic
{
    public class StatisticsManager
    {
        private readonly LiteDatabase _db;
        private readonly ServerStatistics _serverStatistics;
        private readonly PlayerStatistics _playerStatistics;

        public StatisticsManager(LiteDatabase db, ServerStatistics serverStatistics, PlayerStatistics playerStatistics)
        {
            _db = db;
            _serverStatistics = serverStatistics;
            _playerStatistics = playerStatistics;
        }

        public bool PutServerInfo(string endpoint, AdvertiseInfo info)
        {
            if (!endpoint.IsValidEndpoint() || info.IsNotFull()) return false;
            _serverStatistics.PutAdvertise(endpoint, info);
            return true;
        }

        public bool PutMatchInfo(string endpoint, string timestamp, MatchInfo info)
        {
            if (info.IsNotFull() || !endpoint.IsValidEndpoint() ||
                !timestamp.IsValidTimestamp() || !_serverStatistics.HasAdvertise(endpoint) ||
                _serverStatistics.GetMatch(endpoint, timestamp) != null)
            {
                return false;
            }
            using (var transaction = _db.BeginTrans())
            {
                _serverStatistics.PutMatch(endpoint, timestamp, info);
                _playerStatistics.AddMatchInfo(endpoint, timestamp, info);
                transaction.Commit();
            }
            return true;
        }

        public AdvertiseInfo GetServerInfo(string endpoint)
        {
            return _serverStatistics.GetAdvertise(endpoint);
        }

        public MatchInfo GetMatchInfo(string endpoint, string timestamp)
        {
            return _serverStatistics.GetMatch(endpoint, timestamp);
        }

        public List<ServersInfoItem> GetAllServersInfo()
        {
            return _serverStatistics.GetAll();
        }

        public ServerStatsInfo GetServerStats(string endpoint)
        {
            return _serverStatistics.GetStats(endpoint);
        }

        public PlayerStatsInfo GetPlayerStats(string name)
        {
            return _playerStatistics.GetStats(name);
        }

        public List<RecentMatchesItem> GetRecentMatches(int count)
        {
            return _serverStatistics.GetRecentMatches(count);
        }

        public List<BestPlayersItem> GetBestPlayers(int count)
        {
            return _playerStatistics.GetBestPlayers(count);
        }

        public List<PopularServersItem> GetPopularServers(int count)
        {
            return _serverStatistics.GetPopularServers(count);
        }
    }
}