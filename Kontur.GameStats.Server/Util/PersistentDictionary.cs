using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Fclp.Internals.Extensions;
using LiteDB;

namespace Kontur.GameStats.Server.Util
{
    public class PersistentDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        private readonly ConcurrentDictionary<TKey, TValue> _storage;
        private readonly LiteCollection<DbEntry<TKey, TValue>> _dbColl;

        public PersistentDictionary(LiteDatabase db, string collectionName)
        {
            _dbColl = db.GetCollection<DbEntry<TKey, TValue>>(collectionName);
            _storage = new ConcurrentDictionary<TKey, TValue>();
            _dbColl.FindAll().ForEach(e => _storage[e.Key] = e.Value);
        }

        public void Clear()
        {
            _storage.Clear();
            _dbColl.Delete(_ => true);
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            if (!((ICollection<KeyValuePair<TKey, TValue>>)_storage).Remove(item)) return false;
            _dbColl.Delete(item.Key.GetHashCode());
            return true;
        }

        public void Add(TKey key, TValue value)
        {
            ((IDictionary<TKey,TValue>)_storage).Add(key, value);
            _dbColl.Upsert(key.GetHashCode(), DbEntry.Of(key, value));
        }

        public bool Remove(TKey key)
        {
            if (!((IDictionary<TKey, TValue>)_storage).Remove(key)) return false;
            _dbColl.Delete(key.GetHashCode());
            return true;
        }

        public TValue this[TKey key]
        {
            get { return _storage[key]; }
            set
            {
                _storage[key] = value;
                _dbColl.Upsert(key.GetHashCode(), DbEntry.Of(key, value));
            }
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            Add(item.Key, item.Value);
        }

        public int Count => _storage.Count;
        public bool IsReadOnly => ((ICollection<KeyValuePair<TKey, TValue>>)_storage).IsReadOnly;
        public ICollection<TKey> Keys => _storage.Keys;
        public ICollection<TValue> Values => _storage.Values;

        public bool TryGetValue(TKey key, out TValue value)
        {
            return _storage.TryGetValue(key, out value);
        }

        public bool ContainsKey(TKey key)
        {
            return _storage.ContainsKey(key);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return _storage.GetEnumerator();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return _storage.Contains(item);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<TKey, TValue>>)_storage).CopyTo(array, arrayIndex);
        }
    }
}