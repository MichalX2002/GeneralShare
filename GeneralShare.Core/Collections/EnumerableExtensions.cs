using System;
using System.Collections.Generic;

namespace GeneralShare
{
    public static class EnumerableExtensions
    {
        public static int FastSum<TSource>(
            this IEnumerable<TSource> source, Func<TSource, int> selector)
        {
            int result = 0;
            if (source is IList<TSource> list)
            {
                for (int i = 0; i < list.Count; i++)
                    result += selector(list[i]);
            }
            else if (source is IReadOnlyList<TSource> readList)
            {
                for (int i = 0; i < readList.Count; i++)
                    result += selector(readList[i]);
            }
            else
            {
                foreach (var item in source)
                    result += selector(item);
            }
            return result;
        }

        public static TSource FirstMin<TSource, TKey>(
            this IEnumerable<TSource> source, Func<TSource, TKey> keySelector) where TKey : IComparable<TKey>
        {
            bool hasValue = false;
            TKey lastKey = default;
            TSource lastItem = default;

            void Compare(TSource item)
            {
                if (hasValue && keySelector(item).CompareTo(lastKey) >= 0)
                    return;

                hasValue = true;
                lastItem = item;
                lastKey = keySelector(item);
            }

            if (source is IList<TSource> list)
            {
                for (int i = 0, count = list.Count; i < count; i++)
                    Compare(list[i]);
            }
            else if (source is IReadOnlyList<TSource> readList)
            {
                for (int i = 0, count = readList.Count; i < count; i++)
                    Compare(readList[i]);
            }
            else
            {
                foreach (var item in source)
                    Compare(item);
            }
            return lastItem;
        }

        public static TSource FirstMin<TSource, TKey>(
            this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IComparer<TKey> comparer)
        {
            bool hasValue = false;
            TKey lastKey = default;
            TSource lastItem = default;

            void Compare(TSource item)
            {
                if (hasValue && comparer.Compare(keySelector(item), lastKey) >= 0)
                    return;

                hasValue = true;
                lastItem = item;
                lastKey = keySelector(item);
            }

            if (source is IList<TSource> list)
            {
                for (int i = 0, count = list.Count; i < count; i++)
                    Compare(list[i]);
            }
            else if (source is IReadOnlyList<TSource> readList)
            {
                for (int i = 0, count = readList.Count; i < count; i++)
                    Compare(readList[i]);
            }
            else
            {
                foreach (var item in source)
                    Compare(item);
            }
            return lastItem;
        }

        public static T FirstMin<T>(this IEnumerable<T> source) where T : IComparable<T>
        {
            bool hasValue = false;
            T lastItem = default;

            void Compare(T item)
            {
                if (hasValue && item.CompareTo(lastItem) >= 0)
                    return;

                lastItem = item;
                hasValue = true;
            }
            
            if (source is IList<T> list)
            {
                for (int i = 0, count = list.Count; i < count; i++)
                    Compare(list[i]);
            }
            else if (source is IReadOnlyList<T> readList)
            {
                for (int i = 0, count = readList.Count; i < count; i++)
                    Compare(readList[i]);
            }
            else
            {
                foreach (var item in source)
                    Compare(item);
            }
            return lastItem;
        }

        public static T FirstMin<T>(this IEnumerable<T> source, IComparer<T> comparer)
        {
            bool hasValue = false;
            T lastItem = default;

            void Compare(T item)
            {
                if (hasValue && comparer.Compare(item, lastItem) >= 0)
                    return;

                lastItem = item;
                hasValue = true;
            }

            if (source is IList<T> list)
            {
                for (int i = 0, count = list.Count; i < count; i++)
                    Compare(list[i]);
            }
            else if (source is IReadOnlyList<T> readList)
            {
                for (int i = 0, count = readList.Count; i < count; i++)
                    Compare(readList[i]);
            }
            else
            {
                foreach (var item in source)
                    Compare(item);
            }
            return lastItem;
        }
    }
}
