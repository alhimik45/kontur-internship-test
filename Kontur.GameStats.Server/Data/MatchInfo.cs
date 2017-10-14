using System;
using System.Collections.Generic;
using System.Linq;

namespace Kontur.GameStats.Server.Data
{
    /// <summary>
    /// Класс с информацией о результатах матча
    /// </summary>
    [Serializable]
    public class MatchInfo
    {
        public string Map { get; set; }
        public string GameMode { get; set; }
        public int? FragLimit { get; set; }
        public int? TimeLimit { get; set; }
        public double? TimeElapsed { get; set; }
        public List<PlayerMatchInfo> Scoreboard { get; set; }

        /// <summary>
        /// Проверка на заполненность всех полей
        /// </summary>
        /// <returns>true - если все поля присутствуют, false - какое-то поле не заполнено</returns>
        public bool IsNotFull()
        {
            return Map == null || GameMode == null || FragLimit == null ||
                TimeLimit == null || TimeElapsed == null || Scoreboard == null ||
                Scoreboard.Any(info => info.IsNotFull());
        }
    }
}