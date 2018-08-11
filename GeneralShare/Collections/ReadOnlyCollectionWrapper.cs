using System;
using System.Collections;
using System.Collections.Generic;

namespace GeneralShare.Collections
{
    public class ReadOnlyCollectionWrapper<T> : ICollection<T>, IReadOnlyCollection<T>
    {
        private IReadOnlyCollection<T> _collection;
        private HashSet<T> _hashset;
            
        public int Count => _collection.Count;
        public bool IsReadOnly => true;

        public ReadOnlyCollectionWrapper(IReadOnlyCollection<T> collection)
        {
            _collection = collection;
            if (collection is HashSet<T> hashset)
                _hashset = hashset;
        }

        public bool Contains(T item)
        {
            if (_hashset != null)
                return _hashset.Contains(item);

            foreach (var itemInCollection in _collection)
            {
                if (itemInCollection.Equals(item))
                    return true;
            }
            return false;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            if (Count + arrayIndex > array.Length)
                throw new ArgumentException(nameof(array), "Insufficient capacity.");

            foreach (var item in _collection)
                array[arrayIndex++] = item;
        }

        public void Add(T item)
        {
            throw new InvalidOperationException();
        }

        public void Clear()
        {
            throw new InvalidOperationException();
        }

        public bool Remove(T item)
        {
            throw new InvalidOperationException();
        }

        public IEnumerator<T> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
