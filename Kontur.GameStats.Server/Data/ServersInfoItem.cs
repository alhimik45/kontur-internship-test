using System;

namespace Kontur.GameStats.Server.Data
{
    /// <summary>
    /// Класс элемента списка с информацией о всех серверах
    /// </summary>
    [Serializable]
    public class ServersInfoItem
    {
        public string Endpoint { get; set; }
        public AdvertiseInfo Info { get; set; }
    }
}