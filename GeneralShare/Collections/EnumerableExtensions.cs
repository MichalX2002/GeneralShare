using MonoGame.Extended;
using System;
using System.Collections.Generic;

namespace GeneralShare.Collections
{
    public static class EnumerableExtensions
    {
        public static TSource FirstMin<TSource, TKey>(
               this IEnumerable<TSource> source, Func<TSource, TKey> keySelector) where TKey : IComparable<TKey>
        {
            TSource lastItem = default;
            bool hasValue = false;
            
            if (source is IReferenceList<TSource> refList)
            {
                for (int i = 0, count = refList.Count; i < count; i++)
                {
                    ref TSource item = ref refList.GetReferenceAt(i);
                    if (hasValue)
                    {
                        if (keySelector(item).CompareTo(keySelector(lastItem)) < 0)
                            lastItem = item;
                    }
                    else
                    {
                        lastItem = item;
                        hasValue = true;
                    }
                }
            }
            else if (source is IList<TSource> list)
            {
                for (int i = 0, count = list.Count; i < count; i++)
                {
                    TSource item = list[i];
                    if (hasValue)
                    {
                        if (keySelector(item).CompareTo(keySelector(lastItem)) < 0)
                            lastItem = item;
                    }
                    else
                    {
                        lastItem = item;
                        hasValue = true;
                    }
                }
            }
            else if (source is IReadOnlyList<TSource> readList)
            {
                for (int i = 0, count = readList.Count; i < count; i++)
                {
                    TSource item = readList[i];
                    if (hasValue)
                    {
                        if (keySelector(item).CompareTo(keySelector(lastItem)) < 0)
                            lastItem = item;
                    }
                    else
                    {
                        lastItem = item;
                        hasValue = true;
                    }
                }
            }
            else
            {
                foreach (var item in source)
                {
                    if (hasValue)
                    {
                        if (keySelector(item).CompareTo(keySelector(lastItem)) < 0)
                            lastItem = item;
                    }
                    else
                    {
                        lastItem = item;
                        hasValue = true;
                    }
                }
            }

            return lastItem;
        }

        public static TSource FirstMin<TSource, TKey>(
            this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IComparer<TKey> comparer)
        {
            TSource lastItem = default;
            bool hasValue = false;

            if (source is IReferenceList<TSource> refList)
            {
                for (int i = 0, count = refList.Count; i < count; i++)
                {
                    ref TSource item = ref refList.GetReferenceAt(i);
                    if (hasValue)
                    {
                        if (comparer.Compare(keySelector(item), keySelector(lastItem)) < 0)
                            lastItem = item;
                    }
                    else
                    {
                        lastItem = item;
                        hasValue = true;
                    }
                }
            }
            if (source is IList<TSource> list)
            {
                for (int i = 0, count = list.Count; i < count; i++)
                {
                    TSource item = list[i];
                    if (hasValue)
                    {
                        if (comparer.Compare(keySelector(item), keySelector(lastItem)) < 0)
                            lastItem = item;
                    }
                    else
                    {
                        lastItem = item;
                        hasValue = true;
                    }
                }
            }
            else if (source is IReadOnlyList<TSource> readList)
            {
                for (int i = 0, count = readList.Count; i < count; i++)
                {
                    TSource item = readList[i];
                    if (hasValue)
                    {
                        if (comparer.Compare(keySelector(item), keySelector(lastItem)) < 0)
                            lastItem = item;
                    }
                    else
                    {
                        lastItem = item;
                        hasValue = true;
                    }
                }
            }
            else
            {
                foreach (var item in source)
                {
                    if (hasValue)
                    {
                        if (comparer.Compare(keySelector(item), keySelector(lastItem)) < 0)
                            lastItem = item;
                    }
                    else
                    {
                        lastItem = item;
                        hasValue = true;
                    }
                }
            }

            return lastItem;
        }

        public static T FirstMin<T>(this IEnumerable<T> source) where T : IComparable<T>
        {
            T lastItem = default;
            bool hasValue = false;

            if (source is IReferenceList<T> refList)
            {
                for (int i = 0, count = refList.Count; i < count; i++)
                {
                    ref T item = ref refList.GetReferenceAt(i);
                    if (hasValue)
                    {
                        if (item.CompareTo(lastItem) < 0)
                            lastItem = item;
                    }
                    else
                    {
                        lastItem = item;
                        hasValue = true;
                    }
                }
            }
            if (source is IList<T> list)
            {
                for (int i = 0, count = list.Count; i < count; i++)
                {
                    T item = list[i];
                    if (hasValue)
                    {
                        if (item.CompareTo(lastItem) < 0)
                            lastItem = item;
                    }
                    else
                    {
                        lastItem = item;
                        hasValue = true;
                    }
                }
            }
            else if (source is IReadOnlyList<T> readList)
            {
                for (int i = 0, count = readList.Count; i < count; i++)
                {
                    T item = readList[i];
                    if (hasValue)
                    {
                        if (item.CompareTo(lastItem) < 0)
                            lastItem = item;
                    }
                    else
                    {
                        lastItem = item;
                        hasValue = true;
                    }
                }
            }
            else
            {
                foreach (var item in source)
                {
                    if (hasValue)
                    {
                        if (item.CompareTo(lastItem) < 0)
                            lastItem = item;
                    }
                    else
                    {
                        lastItem = item;
                        hasValue = true;
                    }
                }
            }

            return lastItem;
        }

        public static T FirstMin<T>(this IEnumerable<T> source, IComparer<T> comparer)
        {
            T lastItem = default;
            bool hasValue = false;

            if (source is IReferenceList<T> refList)
            {
                for (int i = 0, count = refList.Count; i < count; i++)
                {
                    ref T item = ref refList.GetReferenceAt(i);
                    if (hasValue)
                    {
                        if (comparer.Compare(item, lastItem) < 0)
                            lastItem = item;
                    }
                    else
                    {
                        lastItem = item;
                        hasValue = true;
                    }
                }
            }
            if (source is IList<T> list)
            {
                for (int i = 0, count = list.Count; i < count; i++)
                {
                    T item = list[i];
                    if (hasValue)
                    {
                        if (comparer.Compare(item, lastItem) < 0)
                            lastItem = item;
                    }
                    else
                    {
                        lastItem = item;
                        hasValue = true;
                    }
                }
            }
            else if (source is IReadOnlyList<T> readList)
            {
                for (int i = 0, count = readList.Count; i < count; i++)
                {
                    T item = readList[i];
                    if (hasValue)
                    {
                        if (comparer.Compare(item, lastItem) < 0)
                            lastItem = item;
                    }
                    else
                    {
                        lastItem = item;
                        hasValue = true;
                    }
                }
            }
            else
            {
                foreach (var item in source)
                {
                    if (hasValue)
                    {
                        if (comparer.Compare(item, lastItem) < 0)
                            lastItem = item;
                    }
                    else
                    {
                        lastItem = item;
                        hasValue = true;
                    }
                }
            }

            return lastItem;
        }
    }
}
