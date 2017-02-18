using System;

namespace Kontur.GameStats.Server.Data
{
    [Serializable]
    public class ServersInfoItem
    {
        public string Endpoint { get; set; }
        public AdvertiseInfo Info { get; set; }
    }
}