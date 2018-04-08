using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using Newtonsoft.Json;

namespace Kontur.GameStats.Server.Util
{
    public static class Collections
    {
        /// <summary>
        /// Сохранение коллекции в файл
        /// </summary>
        public static void Save<T>(List<T> collection, string filename)
        {
            using (var fs = new FileStream(filename, FileMode.Create))
            {
                using (var ds = new DeflateStream(fs, CompressionLevel.Fastest))
                {
	                var b = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(collection));
					ds.Write(b, 0, b.Length);
                }
            }
        }

        /// <summary>
        /// Загрузка коллекции из файлы
        /// </summary>
        /// <returns>Загруженную коллекцию</returns>
        public static List<T> Load<T>(string filename)
        {
            if(!File.Exists(filename)) return new List<T>();
            using (var fs = new FileStream(filename, FileMode.Open))
            {
                using (var ds = new DeflateStream(fs, CompressionMode.Decompress))
                {
	                using (var sr = new StreamReader(ds, Encoding.UTF8))
	                {
						return JsonConvert.DeserializeObject<List<T>>(sr.ReadToEnd());
					}
                }
            }
        }
    }
}