using System;
using System.Collections.Generic;
using System.Linq;

namespace Kontur.GameStats.Server.Extensions
{
    public static class Collection
    {
        public static void UpdateTop<TItem, TCmpField, TUniqField>(
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
                if (!insertNewRecent) return;
                coll.Add(item);
            }
            else
            {
                coll[itemIdx] = item;
            }
            coll.Sort((i1, i2) => comparedParam(i2).CompareTo(comparedParam(i1)));
        }
    }
}