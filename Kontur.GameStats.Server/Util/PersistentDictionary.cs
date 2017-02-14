using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Kontur.GameStats.Server.Data;
using LiteDB;

namespace Kontur.GameStats.Server.Util
{
    public class PersistentDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        private readonly Dictionary<TKey, TValue> _storage;
        private readonly LiteCollection<DbEntry<TKey, TValue>> _dbColl;

        public PersistentDictionary(LiteDatabase db, string collectionName)
        {
            _dbColl = db.GetCollection<DbEntry<TKey, TValue>>(collectionName);
            _storage = _dbColl.FindAll().ToDictionary(e => e.Key, e => e.Value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return _storage.GetEnumerator();
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            Add(item.Key, item.Value);
        }

        public void Clear()
        {
            _dbColl.Delete(_ => true);
            _storage.Clear();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return _storage.Contains(item);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<TKey, TValue>>)_storage).CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            if (!((ICollection<KeyValuePair<TKey, TValue>>)_storage).Remove(item)) return false;
            _dbColl.Delete(item.Key.GetHashCode());
            return true;
        }

        public int Count => _storage.Count;
        public bool IsReadOnly => ((ICollection<KeyValuePair<TKey, TValue>>)_storage).IsReadOnly;
        public bool ContainsKey(TKey key)
        {
            return _storage.ContainsKey(key);
        }

        public void Add(TKey key, TValue value)
        {
            _dbColl.Upsert(key.GetHashCode(), new DbEntry<TKey, TValue>(key, value));
            _storage.Add(key, value);
        }

        public bool Remove(TKey key)
        {
            if (!_storage.Remove(key)) return false;
            _dbColl.Delete(key.GetHashCode());
            return true;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return _storage.TryGetValue(key, out value);
        }

        public TValue this[TKey key]
        {
            get { return _storage[key]; }
            set
            {
                _dbColl.Upsert(key.GetHashCode(), new DbEntry<TKey, TValue>(key, value));
                _storage[key] = value;
            }
        }

        public ICollection<TKey> Keys => _storage.Keys;
        public ICollection<TValue> Values => _storage.Values;
    }
}