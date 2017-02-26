using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization.Formatters.Binary;

namespace Kontur.GameStats.Server.Util
{
    public static class Collections
    {
        /// <summary>
        /// Сохранение коллекции в файл
        /// </summary>
        public static void Save<T>(List<T> collection, string filename)
        {
            var formatter = new BinaryFormatter();
            using (var fs = new FileStream(filename, FileMode.Create))
            {
                using (var ds = new DeflateStream(fs, CompressionLevel.Fastest))
                {
                    formatter.Serialize(ds, collection);
                }
            }
        }

        /// <summary>
        /// Загрузка коллекции из файлы
        /// </summary>
        /// <returns>Загруженную коллекцию</returns>
        public static List<T> Load<T>(string filename)
        {
            var formatter = new BinaryFormatter();
            if(!File.Exists(filename)) return new List<T>();
            using (var fs = new FileStream(filename, FileMode.Open))
            {
                using (var ds = new DeflateStream(fs, CompressionMode.Decompress))
                {
                    return (List<T>) formatter.Deserialize(ds);
                }
            }
        }
    }
}