using System;

namespace Kontur.GameStats.Server.Data
{
    /// <summary>
    /// Класс с информацией о результатах одного игрока в матче
    /// </summary>
    [Serializable]
    public class PlayerMatchInfo
    {
        public string Name { get; set; }
        public int? Frags { get; set; }
        public int? Kills { get; set; }
        public int? Deaths { get; set; }

        /// <summary>
        /// Проверка на заполненность всех полей
        /// </summary>
        /// <returns>true - если все поля присутствуют, false - какое-то поле не заполнено</returns>
        public bool IsNotFull()
        {
            return Name == null || Frags == null || Kills == null || Deaths == null;
        }
    }
}