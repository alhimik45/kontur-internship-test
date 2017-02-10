using System;
using System.Collections.Generic;
using System.Linq;
using Kontur.GameStats.Server.Data;

namespace Kontur.GameStats.Server
{
    public class StatisticsManager
    {
        
        private readonly ServerStatistics _serverStatistics;
        private readonly PlayerStatistics _playerStatistics;

        public StatisticsManager(ServerStatistics serverStatistics, PlayerStatistics playerStatistics)
        {
            Console.WriteLine("kekd");
            _serverStatistics = serverStatistics;
            _playerStatistics = playerStatistics;
        }

        private bool IsValidEndpoint(string endpoint)
        {
            var parts = endpoint.Split("-".ToCharArray());
            if (parts.Length != 2) return false;
            if (Uri.CheckHostName(parts[0]) == UriHostNameType.Unknown) return false;
            int port;
            return int.TryParse(parts[1], out port);
        }

        private bool IsValidTimestamp(string timestamp)
        {
            DateTime date;
            return DateTime.TryParseExact(timestamp, "yyyy-MM-ddTHH:mm:ssZ", null,
                System.Globalization.DateTimeStyles.None, out date);
        }

        public bool PutServerInfo(string endpoint, AdvertiseInfo info)
        {
            if (!IsValidEndpoint(endpoint) || info.IsNotFull()) return false;
            _serverStatistics.PutAdvertise(endpoint, info);
            return true;
        }

        public bool PutMatchInfo(string endpoint, string timestamp, MatchInfo info)
        {
            if (info.IsNotFull() || !IsValidEndpoint(endpoint) ||
                !IsValidTimestamp(timestamp) || !_serverStatistics.HasAdvertise(endpoint) ||
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
            return _serverStatistics.GetMatch(endpoint,timestamp);
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