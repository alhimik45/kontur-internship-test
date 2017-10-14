using System;

namespace Kontur.GameStats.Server.Data
{
    /// <summary>
    /// ����� �������� ������ ������ �������
    /// </summary>
    [Serializable]
    public class BestPlayersItem
    {
        public string Name { get; set; }
        public double KillToDeathRatio { get; set; }
    }
}