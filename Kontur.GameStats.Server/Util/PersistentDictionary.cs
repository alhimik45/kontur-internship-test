using System;
using System.Collections;
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
    public class PersistentDictionary<TValue> : IEnumerable<KeyValuePair<string, TValue>>
    {
        private readonly bool _noKeyFolder;
        private readonly string _innerValuesDirName = Path.DirectorySeparatorChar + "Inner";
        private readonly string _collectionName;
        private readonly string _basePath;
        private readonly ConcurrentDictionary<string, TValue> _storage;

        public PersistentDictionary(string basePath, string collectionName, bool doubleKey = false, bool noKeyFolder = false)
        {
            _noKeyFolder = noKeyFolder;
            _collectionName = Path.DirectorySeparatorChar + collectionName;
            _basePath = basePath + Path.DirectorySeparatorChar;
            _storage = new ConcurrentDictionary<string, TValue>();
            var formatter = new BinaryFormatter();
            Directory.CreateDirectory(basePath);
            foreach (var outDir in Directory.GetDirectories(basePath))
            {
                if (noKeyFolder)
                {
                    foreach (var file in Directory.GetFiles(outDir))
                    {
                        var key = GetUnescapedName(file);
                        ReadValue(file, key, formatter);
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
                            ReadValue(dir + _collectionName, key, formatter);
                        }
                    }
                }

            }
        }

        private static string GetUnescapedName(string dir)
        {
            return Uri.UnescapeDataString(dir.Split(Path.DirectorySeparatorChar).Last());
        }

        private void ReadValue(string file, string key, IFormatter formatter)
        {
            if (!File.Exists(file)) return;
            try
            {
                using (var fs = new FileStream(file, FileMode.Open))
                {
                    using (var ds = new DeflateStream(fs, CompressionMode.Decompress))
                    {
                        _storage[key] = (TValue)formatter.Deserialize(ds);
                    }
                }
            }
            catch (SerializationException)
            {
                //if there bad data, delete it
                File.Delete(file);
            }
        }

        private void ReadInnerValues(string dir, string key, IFormatter formatter)
        {
            if (!Directory.Exists(dir)) return;
            foreach (var file in Directory.GetFiles(dir))
            {
                var innerKey = Uri.UnescapeDataString(file.Split(Path.DirectorySeparatorChar).Last());
                try
                {
                    using (var fs = new FileStream(file, FileMode.Open))
                    {
                        using (var ds = new DeflateStream(fs, CompressionMode.Decompress))
                        {
                            _storage[key + Path.DirectorySeparatorChar + innerKey] =
                                (TValue)formatter.Deserialize(ds);
                        }
                    }
                }
                catch (SerializationException)
                {
                    //if there bad data, delete it
                    File.Delete(file);
                }
            }
        }

        private static string GetKeyPath(string key)
        {
            var dirName = Math.Abs(key.GetHashCode() % 3000).ToString();
            return dirName + Path.DirectorySeparatorChar + Uri.EscapeDataString(key);
        }

        public TValue this[string key]
        {
            get { return _storage.Get(key); }
            set
            {
                _storage[key] = value;
                var path = _basePath;
                string filepath;
                if (_noKeyFolder)
                {
                    path += GetKeyPath(key);
                    filepath = path;
                }
                else
                {
                    path += GetKeyPath(key) + Path.DirectorySeparatorChar;
                    filepath = path + _collectionName;
                }
                // ReSharper disable once AssignNullToNotNullAttribute
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                var formatter = new BinaryFormatter();
                using (var fs = new FileStream(filepath, FileMode.Create))
                {
                    using (var ds = new DeflateStream(fs, CompressionLevel.Fastest))
                    {
                        formatter.Serialize(ds, value);
                    }
                }
            }
        }

        public TValue this[string key1, string key2]
        {
            get { return _storage.Get(key1 + Path.DirectorySeparatorChar + key2); }
            set
            {
                _storage[key1 + Path.DirectorySeparatorChar + key2] = value;
                var innerPath = _basePath + GetKeyPath(key1) + _innerValuesDirName;
                var innerFile = innerPath + Path.DirectorySeparatorChar + Uri.EscapeDataString(key2);
                Directory.CreateDirectory(innerPath);
                var formatter = new BinaryFormatter();
                using (var fs = new FileStream(innerFile, FileMode.Create))
                {
                    using (var ds = new DeflateStream(fs, CompressionLevel.Fastest))
                    {
                        formatter.Serialize(ds, value);
                    }
                }
            }
        }

        public bool TryGetValue(string key, out TValue value)
        {
            return _storage.TryGetValue(key, out value);
        }

        public bool ContainsKey(string key)
        {
            return _storage.ContainsKey(key);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<KeyValuePair<string, TValue>> GetEnumerator()
        {
            return _storage.GetEnumerator();
        }
    }
}