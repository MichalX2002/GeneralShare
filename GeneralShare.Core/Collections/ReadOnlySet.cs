using System.Collections;
using System.Collections.Generic;

namespace GeneralShare.Collections
{
    public class ReadOnlySet<T> : IReadOnlyCollection<T>
    {
        private HashSet<T> _set;

        public int Count => _set.Count;
        
        public ReadOnlySet(HashSet<T> set)
        {
            _set = set;
        }
        
        public bool Contains(T item)
        {
            return _set.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _set.CopyTo(array, arrayIndex);
        }

        public HashSet<T>.Enumerator GetEnumerator() => _set.GetEnumerator();
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
