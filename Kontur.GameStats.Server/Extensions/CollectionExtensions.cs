using System;
using System.Collections.Generic;
using System.Linq;

namespace Kontur.GameStats.Server.Extensions
{
    public static class Collection
    {
        public static List<TItem> UpdateTop<TItem, TCmpField, TUniqField>(
            this List<TItem> coll,
            int maxTopSize,
            Func<TItem, TCmpField> comparedParam,
            Func<TItem, TUniqField> uniqParam, TItem item) where TCmpField : IComparable where TUniqField : IComparable
        {
            var itemIdx = coll.FindIndex(i => uniqParam(i).CompareTo(uniqParam(item)) == 0);
            if (itemIdx == -1)
            {
                var insertNewRecent = coll.Count < maxTopSize;
                if (!insertNewRecent && coll.Count > 0 &&
                    comparedParam(coll.Last()).CompareTo(comparedParam(item)) < 0)
                {
                    coll.RemoveAt(coll.Count - 1);
                    insertNewRecent = true;
                }
                if (!insertNewRecent) return coll;
            }
            else
            {
                coll.RemoveAt(itemIdx);
            }
            itemIdx = coll.BinarySearch(item, Comparer<TItem>.Create((i1, i2) => comparedParam(i2).CompareTo(comparedParam(i1))));
            if (itemIdx < 0)
            {
                itemIdx = ~itemIdx;
            }
            coll.Insert(itemIdx, item);
            return coll;
        }

        public static TValue Get<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key)
        {
            TValue val;
            dict.TryGetValue(key, out val);
            return val;
        }
    }
}