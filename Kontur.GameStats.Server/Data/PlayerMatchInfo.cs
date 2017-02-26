using System;

namespace Kontur.GameStats.Server.Data
{
    /// <summary>
    /// ����� � ����������� � ����������� ������ ������ � �����
    /// </summary>
    [Serializable]
    public class PlayerMatchInfo
    {
        public string Name { get; set; }
        public int? Frags { get; set; }
        public int? Kills { get; set; }
        public int? Deaths { get; set; }

        /// <summary>
        /// �������� �� ������������� ���� �����
        /// </summary>
        /// <returns>true - ���� ��� ���� ������������, false - �����-�� ���� �� ���������</returns>
        public bool IsNotFull()
        {
            return Name == null || Frags == null || Kills == null || Deaths == null;
        }
    }
}