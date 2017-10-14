using System;
using System.Collections.Generic;
using System.Linq;

namespace Kontur.GameStats.Server.Util
{
    /// <summary>
    /// Класс с методами расширениями для коллекций
    /// </summary>
    public static class Collection
    {
        /// <summary>
        /// Обновление коллекции-рейтинга: вставляет в коллекцию новый элемент так,
        /// чтобы список оставался отсортирован по определённому критерию
        /// Также проверяется уникальность элементов по определённому критерию
        /// </summary>
        /// <typeparam name="TItem">Тип элементов коллекции</typeparam>
        /// <typeparam name="TCmpField">Тип значения, по которому сортируется список</typeparam>
        /// <typeparam name="TUniqField">Тип значения, по которому определяется уникальность</typeparam>
        /// <param name="coll">Обновляемая коллекция</param>
        /// <param name="maxTopSize">Максимальный размер коллекции</param>
        /// <param name="comparedParam">Функция для получения из элемента значения для сравнения</param>
        /// <param name="uniqParam">Функция для получения из элемента значения для проверки уникальности</param>
        /// <param name="item">Вставляемое значение</param>
        /// <returns>Эту же коллекцию</returns>
        public static IList<TItem> UpdateTop<TItem, TCmpField, TUniqField>(
            this IList<TItem> coll,
            int maxTopSize,
            Func<TItem, TCmpField> comparedParam,
            Func<TItem, TUniqField> uniqParam, TItem item) where TCmpField : IComparable where TUniqField : IComparable
        {
            var itemIdx = coll.ToList().FindIndex(i => uniqParam(i).CompareTo(uniqParam(item)) == 0);
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
            itemIdx = coll.ToList().BinarySearch(item, Comparer<TItem>.Create((i1, i2) => comparedParam(i2).CompareTo(comparedParam(i1))));
            if (itemIdx < 0)
            {
                itemIdx = ~itemIdx;
            }
            coll.Insert(itemIdx, item);
            return coll;
        }

        /// <summary>
        /// Получение значения из словаря по ключу
        /// В случае остутствия значения возвращается значение по-умолчанию
        /// </summary>
        public static TValue Get<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key)
        {
            TValue val;
            dict.TryGetValue(key, out val);
            return val;
        }
    }
}