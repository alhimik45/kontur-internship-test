using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LiteDB;

namespace Kontur.GameStats.Server.Util
{
    public class PersistentList<T> : IList<T>
    {
        private readonly List<T> _storage;
        private readonly LiteCollection<DbEntry<int, T>> _dbColl;

        public PersistentList(LiteDatabase db, string collectionName)
        {
            _dbColl = db.GetCollection<DbEntry<int, T>>(collectionName);
            _storage = _dbColl.FindAll().OrderBy(e => e.Key).Select(e => e.Value).ToList();
        }

        public void Add(T item)
        {
            _storage.Add(item);
            var index = _storage.Count - 1;
            _dbColl.Upsert(index.GetHashCode(), DbEntry.Of(index, item));
        }

        public void Clear()
        {
            _dbColl.Delete(_ => true);
            _storage.Clear();
        }

        public bool Remove(T item)
        {
            var index = IndexOf(item);
            if (index < 0) return false;
            RemoveAt(index);
            return true;
        }

        public void Insert(int index, T item)
        {
            if (index < 0 || index > _storage.Count)//TODO insert before
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            for (var i = _storage.Count; i > index; i--)
            {
                _dbColl.Upsert(i.GetHashCode(), DbEntry.Of(i, _storage[i - 1]));
            }
            _dbColl.Upsert(index.GetHashCode(), DbEntry.Of(index, item));//TODO Upsert ext
            _storage.Insert(index, item);
        }

        public void RemoveAt(int index)
        {

            for (var i = index; i < _storage.Count - 1; i++)
            {
                _dbColl.Upsert(i.GetHashCode(), DbEntry.Of(i, _storage[i + 1]));
            }
            _dbColl.Delete(_storage.Count - 1);
            _storage.RemoveAt(index);
        }

        public T this[int index]
        {
            get { return _storage[index]; }
            set
            {
                _storage[index] = value;
                _dbColl.Upsert(index.GetHashCode(), DbEntry.Of(index, value));
            }
        }

        public int Count => _storage.Count;
        public bool IsReadOnly => ((ICollection<T>)_storage).IsReadOnly;

        public int IndexOf(T item)
        {
            return _storage.IndexOf(item);
        }

        public bool Contains(T item)
        {
            return _storage.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _storage.CopyTo(array, arrayIndex);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _storage.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

    }
}