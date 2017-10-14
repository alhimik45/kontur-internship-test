using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using FileMode = System.IO.FileMode;

namespace Kontur.GameStats.Server.Util
{
    /// <summary>
    /// Класс словаря, сохраняющего своё состояние на диск
    /// Имеет два режима работы:
    /// 1) простой ключ: ключ - одна строка
    ///     имеет ещё два варианта работы:
    ///     1. ключ - папка, а в ней файл с именем в виде названия коллекции
    ///     2. ключ - файл: используется, если по данным ключам не хранится несколько коллекций сразу
    /// 2) двойной ключ: ключ - две строки
    /// Также есть возможность зеркалирования в памяти:
    /// все значения также хранятся в памяти и при запросе на чтение обращении к диску нет
    /// </summary>
    public class PersistentDictionary<TValue>
    {
        private readonly bool _noKeyFolder;
        private readonly bool _memoryMirror;
        private readonly string _innerValuesDirName = Path.DirectorySeparatorChar + "Inner";
        private readonly string _collectionName;
        private readonly string _basePath;
        private ConcurrentDictionary<string, TValue> _storage;

        /// <param name="basePath">Папка, где будет храниться остальная информация словаря</param>
        /// <param name="collectionName">Имя коллекции, используется для имени файла для хранения данных</param>
        /// <param name="doubleKey">Используется ли в коллекции двойной ключ</param>
        /// <param name="noKeyFolder">Является ли ключ папкой</param>
        /// <param name="memoryMirror">Есть ли зеркалирование в памяти</param>
        public PersistentDictionary(string basePath, string collectionName, bool doubleKey = false, bool noKeyFolder = false, bool memoryMirror = false)
        {
            _noKeyFolder = noKeyFolder;
            _memoryMirror = memoryMirror;
            _collectionName = Path.DirectorySeparatorChar + collectionName;
            _basePath = basePath + Path.DirectorySeparatorChar;

            if (memoryMirror)
            {
                InitMemoryStorage(doubleKey);
            }
        }

        /// <summary>
        /// Загрузка значений коллекции, если они есть в память
        /// </summary>
        /// <param name="doubleKey">Используется ли в коллекции двойной ключ</param>
        private void InitMemoryStorage(bool doubleKey)
        {
            _storage = new ConcurrentDictionary<string, TValue>();

            var formatter = new BinaryFormatter();
            Directory.CreateDirectory(_basePath);

            foreach (var outDir in Directory.GetDirectories(_basePath))
            {
                if (_noKeyFolder)
                {
                    foreach (var file in Directory.GetFiles(outDir))
                    {
                        var key = GetUnescapedName(file);
                        var value = ReadValue(file, formatter);
                        if (!EqualityComparer<TValue>.Default.Equals(value, default(TValue)))
                        {
                            _storage[key] = value;
                        }
                    }
                }
                else
                {
                    foreach (var dir in Directory.GetDirectories(outDir))
                    {
                        var key = GetUnescapedName(dir);
                        if (doubleKey)
                        {
                            ReadInnerValues(dir + _innerValuesDirName, key, formatter);
                        }
                        else
                        {
                            var value = ReadValue(dir + _collectionName, formatter);
                            if (!EqualityComparer<TValue>.Default.Equals(value, default(TValue)))
                            {
                                _storage[key] = value;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Достаёт из пути имя последнего элемента и убирает экранизацию символов с него
        /// </summary>
        /// <returns>Название последнего элемента без экранизации</returns>
        private static string GetUnescapedName(string path)
        {
            return Uri.UnescapeDataString(path.Split(Path.DirectorySeparatorChar).Last());
        }

        /// <summary>
        /// Считывание данных в режиме двойной ключ
        /// </summary>
        /// <param name="dir">Путь до папки считывания</param>
        /// <param name="key">Первый клбч из двух</param>
        /// <param name="formatter">Десериализатор</param>
        private void ReadInnerValues(string dir, string key, IFormatter formatter)
        {
            if (!Directory.Exists(dir)) return;
            foreach (var file in Directory.GetFiles(dir))
            {
                var innerKey = Uri.UnescapeDataString(file.Split(Path.DirectorySeparatorChar).Last());
                var value = ReadValue(file, formatter);
                if (!EqualityComparer<TValue>.Default.Equals(value, default(TValue)))
                {
                    _storage[key + Path.DirectorySeparatorChar + innerKey] = value;
                }
            }
        }

        /// <summary>
        /// Чтение значения из файла
        /// </summary>
        /// <param name="file">Путь к файлу</param>
        /// <param name="formatter">Десериализатор</param>
        /// <returns>Десериализованное значение или значение по-умолчанию, если считать не удалось</returns>
        private static TValue ReadValue(string file, IFormatter formatter)
        {
            if (!File.Exists(file)) return default(TValue);
            try
            {
                using (var fs = new FileStream(file, FileMode.Open))
                {
                    using (var ds = new DeflateStream(fs, CompressionMode.Decompress))
                    {
                        return (TValue)formatter.Deserialize(ds);
                    }
                }
            }
            catch (SerializationException)
            {
                //удаляем поврежденные данные
                File.Delete(file);
                return default(TValue);
            }
        }

        /// <summary>
        /// Запись значения в файл
        /// </summary>
        private static void WriteValue(TValue value, string file)
        {
            var formatter = new BinaryFormatter();
            using (var fs = new FileStream(file, FileMode.Create))
            {
                using (var ds = new DeflateStream(fs, CompressionLevel.Fastest))
                {
                    formatter.Serialize(ds, value);
                }
            }
        }

        /// <summary>
        /// Получение пути по ключу
        /// Путь состоит из двух частей:
        /// 1) Остаток от хеша ключа
        /// 2) Экранированое значение самого ключа
        /// Это сделано для уменьшения количества элементов в одной папке для увеличения производительности
        /// </summary>
        /// <param name="key">Ключ</param>
        /// <returns>Строку вида "число/экранированный_ключ"</returns>
        private static string GetKeyPath(string key)
        {
            var dirName = Math.Abs(key.GetHashCode() % 3000).ToString();
            return dirName + Path.DirectorySeparatorChar + Uri.EscapeDataString(key);
        }

        /// <returns>Полный путь к папке, соответствующей данному ключу</returns>
        private string GetFullElementPath(string key)
        {
            var path = _basePath + GetKeyPath(key);
            if (!_noKeyFolder)
            {
                path += Path.DirectorySeparatorChar;
            }
            return path;
        }

        /// <returns>Путь к файлу текущей коллекции, в который нужно сохранять значение</returns>
        private string GetFullElementFilename(string path)
        {
            return _noKeyFolder ? path : path + _collectionName;
        }

        /// <summary>
        /// Получение или добавление значения по ключу
        /// </summary>
        public TValue this[string key]
        {
            get
            {
                if (_memoryMirror)
                {
                    return _storage.Get(key);
                }
                var path = GetFullElementPath(key);
                var filepath = GetFullElementFilename(path);
                return ReadValue(filepath, new BinaryFormatter());
            }
            set
            {
                if (_memoryMirror)
                {
                    _storage[key] = value;
                }
                var path = GetFullElementPath(key);
                var filepath = GetFullElementFilename(path);

                // ReSharper disable once AssignNullToNotNullAttribute
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                WriteValue(value, filepath);
            }
        }

        /// <summary>
        /// Получение или добавление значения по двойному ключу
        /// </summary>
        public TValue this[string key1, string key2]
        {
            get
            {
                var key = key1 + Path.DirectorySeparatorChar + key2;
                if (_memoryMirror)
                {
                    return _storage.Get(key);
                }
                var innerPath = _basePath + GetKeyPath(key1) + _innerValuesDirName;
                var innerFile = innerPath + Path.DirectorySeparatorChar + Uri.EscapeDataString(key2);
                return ReadValue(innerFile, new BinaryFormatter());
            }
            set
            {
                if (_memoryMirror)
                {
                    _storage[key1 + Path.DirectorySeparatorChar + key2] = value;
                }
                var innerPath = _basePath + GetKeyPath(key1) + _innerValuesDirName;
                var innerFile = innerPath + Path.DirectorySeparatorChar + Uri.EscapeDataString(key2);
                Directory.CreateDirectory(innerPath);
                WriteValue(value, innerFile);
            }
        }

        /// <summary>
        /// Пытается получить значение, связанное с указанным ключом
        /// </summary>
        /// <param name="key">Ключ значения, которое необходимо получить.</param>
        /// <param name="value">
        /// Параметр, возвращаемый этим методом, содержит объект из коллекции с 
        /// заданным ключом или значение по умолчанию, если операцию не удалось выполнить.
        /// </param>
        /// <returns>true - если ключ был найден в коллекции, false - если нет</returns>
        public bool TryGetValue(string key, out TValue value)
        {
            if (_memoryMirror)
            {
                return _storage.TryGetValue(key, out value);
            }
            value = this[key];
            return !EqualityComparer<TValue>.Default.Equals(value, default(TValue));
        }

        /// <returns>true - если есть значение, соответствующее данному ключу, false - если нет</returns>
        public bool ContainsKey(string key)
        {
            if (_memoryMirror)
            {
                return _storage.ContainsKey(key);
            }
            var path = GetFullElementPath(key);
            var filepath = GetFullElementFilename(path);
            return File.Exists(filepath);
        }

        /// <summary>
        /// Проецирует каждый элемент коллекции из памяти в новую форму
        /// </summary>
        /// <returns>Результирующий список</returns>
        /// <exception cref="InvalidOperationException">
        /// Выбрасывается при попытке обойти коллекцию, которая не хранится в памяти
        /// </exception>
        public List<TRes> Select<TRes>(Func<KeyValuePair<string, TValue>, TRes> mapFunc)
        {
            if (!_memoryMirror)
            {
                throw new InvalidOperationException("Cannot enumerate collection that not in memory");
            }
            return _storage.Select(mapFunc).ToList();
        }
    }
}