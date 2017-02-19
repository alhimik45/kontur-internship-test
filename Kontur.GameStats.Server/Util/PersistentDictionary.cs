using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Kontur.GameStats.Server.Extensions;
using FileMode = System.IO.FileMode;

namespace Kontur.GameStats.Server.Util
{
    public class PersistentDictionary<TValue> : IEnumerable<KeyValuePair<string, TValue>>
    {
        private readonly string _innerValues = Path.DirectorySeparatorChar + "Inner";
        private readonly string _collectionName;
        private readonly string _basePath;
        private readonly ConcurrentDictionary<string, TValue> _storage;

        public PersistentDictionary(string basePath, string collectionName, bool doubleKey)
        {
            _collectionName = Path.DirectorySeparatorChar + collectionName;
            _basePath = basePath + Path.DirectorySeparatorChar;
            _storage = new ConcurrentDictionary<string, TValue>();
            var formatter = new BinaryFormatter();
            Directory.CreateDirectory(basePath);
            foreach (var outDir in Directory.GetDirectories(basePath))
            {
                foreach (var dir in Directory.GetDirectories(outDir))
                {
                    var key = Uri.UnescapeDataString(dir.Split(Path.DirectorySeparatorChar).Last());
                    if (doubleKey)
                    {
                        ReadInnerValues(dir + _innerValues, key, formatter);
                    }
                    else
                    {
                        ReadValue(dir, key, formatter);
                    }
                }
            }
        }

        private void ReadValue(string dir, string key, IFormatter formatter)
        {
            var filePath = dir + _collectionName;
            if (!File.Exists(filePath)) return;
            try
            {
                using (var fs = new FileStream(filePath, FileMode.Open))
                {
                    _storage[key] = (TValue)formatter.Deserialize(fs);
                }
            }
            catch (SerializationException)
            {
                //if there bad data, delete it
                File.Delete(filePath);
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
                        _storage[key + Path.DirectorySeparatorChar + innerKey] =
                            (TValue)formatter.Deserialize(fs);
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
            return Math.Abs(key.GetHashCode()).ToString().Substring(0, 3) + Path.DirectorySeparatorChar + Uri.EscapeDataString(key);
        }

        public TValue this[string key]
        {
            get { return _storage.Get(key); }
            set
            {
                _storage[key] = value;
                var path = _basePath + GetKeyPath(key) + Path.DirectorySeparatorChar;
                Directory.CreateDirectory(path);
                var formatter = new BinaryFormatter();
                using (var fs = new FileStream(path + _collectionName, FileMode.Create))
                {
                    formatter.Serialize(fs, value);
                }
            }
        }

        public TValue this[DoubleKey key]
        {
            get { return _storage.Get(key.ToString()); }
            set
            {
                _storage[key.ToString()] = value;
                var innerPath = _basePath + GetKeyPath(key.Key1) + _innerValues;
                var innerFile = innerPath + Path.DirectorySeparatorChar + Uri.EscapeDataString(key.Key2);
                Directory.CreateDirectory(innerPath);
                var formatter = new BinaryFormatter();
                using (var fs = new FileStream(innerFile, FileMode.Create))
                {
                    formatter.Serialize(fs, value);
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