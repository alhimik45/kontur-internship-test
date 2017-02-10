using System.Collections.Generic;

namespace Kontur.GameStats.Server.Data
{
    public class AdvertiseInfo
    {
        public string Name { get; set; }
        public List<string> GameModes { get; set; }

        public bool IsNotFull()
        {
            return Name == null || GameModes == null;
        }
    }
}