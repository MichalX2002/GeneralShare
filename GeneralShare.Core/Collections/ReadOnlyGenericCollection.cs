using System;
using System.Collections;
using System.Collections.Generic;

namespace GeneralShare.Collections
{
    public class ReadOnlyWrapper<T> : ICollection<T>, IReadOnlyCollection<T>
    {
        private ICollection<T> _collection;

        public int Count => _collection.Count;
        public bool IsReadOnly => true;

        public ReadOnlyWrapper(ICollection<T> collection)
        {
            _collection = collection ?? throw new ArgumentNullException(nameof(collection));
        }
        
        public ReadOnlyWrapper(ISet<T> set) : this(set as ICollection<T>)
        {
        }
        
        public bool Contains(T item)
        {
            return _collection.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _collection.CopyTo(array, arrayIndex);
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
            return _collection.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
