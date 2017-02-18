using System;

namespace Kontur.GameStats.Server.Data
{
    [Serializable]
    public class PopularServersItem
    {
        public string Endpoint { get; set; }
        public string Name { get; set; }
        public double AverageMatchesPerDay { get; set; }
    }
}