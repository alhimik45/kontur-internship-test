using System.Collections.Generic;
using Kontur.GameStats.Server.Data;
using Kontur.GameStats.Server.Extensions;

namespace Kontur.GameStats.Server.Logic
{
    public class StatisticsManager
    {
        private readonly ServerStatistics _serverStatistics;
        private readonly PlayerStatistics _playerStatistics;

        public StatisticsManager(ServerStatistics serverStatistics, PlayerStatistics playerStatistics)
        {
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

            _serverStatistics.PutMatch(endpoint, timestamp, info);
            _playerStatistics.AddMatchInfo(endpoint, timestamp, info);
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

        public PublicServerStats GetServerStats(string endpoint)
        {
            return _serverStatistics.GetStats(endpoint);
        }

        public PublicPlayerStats GetPlayerStats(string name)
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