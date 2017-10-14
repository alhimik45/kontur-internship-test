using System;
using System.Collections.Generic;

namespace Kontur.GameStats.Server.Data
{
    /// <summary>
    /// Класс с информацией advertise-запроса сервера
    /// </summary>
    [Serializable]
    public class AdvertiseInfo
    {
        public string Name { get; set; }
        public List<string> GameModes { get; set; }

        /// <summary>
        /// Проверка на заполненность всех полей
        /// </summary>
        /// <returns>true - если все поля присутствуют, false - какое-то поле не заполнено</returns>
        public bool IsNotFull()
        {
            return Name == null || GameModes == null;
        }
    }
}