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
    public class PersistentDictionary<TValue>
    {
        private readonly bool _noKeyFolder;
        private readonly bool _memoryMirror;
        private readonly string _innerValuesDirName = Path.DirectorySeparatorChar + "Inner";
        private readonly string _collectionName;
        private readonly string _basePath;
        private ConcurrentDictionary<string, TValue> _storage;

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

        private static string GetUnescapedName(string dir)
        {
            return Uri.UnescapeDataString(dir.Split(Path.DirectorySeparatorChar).Last());
        }

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

        private static string GetKeyPath(string key)
        {
            var dirName = Math.Abs(key.GetHashCode() % 3000).ToString();
            return dirName + Path.DirectorySeparatorChar + Uri.EscapeDataString(key);
        }

        private string GetFullElementPath(string key)
        {
            var path = _basePath + GetKeyPath(key);
            if (!_noKeyFolder)
            {
                path += Path.DirectorySeparatorChar;
            }
            return path;
        }

        private string GetFullElementFilename(string path)
        {
            return _noKeyFolder ? path : path + _collectionName;
        }

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

        public bool TryGetValue(string key, out TValue value)
        {
            if (_memoryMirror)
            {
                return _storage.TryGetValue(key, out value);
            }
            value = this[key];
            return !EqualityComparer<TValue>.Default.Equals(value, default(TValue));
        }

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