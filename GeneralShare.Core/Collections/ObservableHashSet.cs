using System.Collections;
using System.Collections.Generic;

namespace GeneralShare.Collections
{
    public class ObservableHashSet<T> : ISet<T>
    {
        public delegate void ChangeDelegate(ObservableHashSet<T> sender, T value);

        private HashSet<T> _set;

        public int Count => _set.Count;
        public bool IsReadOnly => ((ISet<T>)_set).IsReadOnly;

        public IEqualityComparer<T> Comparer => _set.Comparer;

        public event ChangeDelegate OnAdd;
        public event ChangeDelegate OnRemove;

        public ObservableHashSet()
        {
            _set = new HashSet<T>();
        }

        public ObservableHashSet(int capacity)
        {
            _set = new HashSet<T>(capacity);
        }

        public ObservableHashSet(IEnumerable<T> collection)
        {
            _set = new HashSet<T>(collection);
        }

        public ObservableHashSet(IEqualityComparer<T> comparer)
        {
            _set = new HashSet<T>(comparer);
        }

        public ObservableHashSet(int capacity, IEqualityComparer<T> comparer)
        {
            _set = new HashSet<T>(capacity, comparer);
        }

        public ObservableHashSet(IEnumerable<T> collection, IEqualityComparer<T> comparer)
        {
            _set = new HashSet<T>(collection, comparer);
        }

        public bool Add(T item)
        {
            if (_set.Add(item))
            {
                OnAdd?.Invoke(this, item);
                return true;
            }
            return false;
        }

        void ICollection<T>.Add(T item)
        {
            Add(item);
        }

        public bool Remove(T item)
        {
            if (_set.Remove(item))
            {
                OnRemove?.Invoke(this, item);
                return true;
            }
            return false;
        }

        public void UnionWith(IEnumerable<T> other)
        {
            _set.UnionWith(other);
        }

        public void IntersectWith(IEnumerable<T> other)
        {
            _set.IntersectWith(other);
        }

        public void ExceptWith(IEnumerable<T> other)
        {
            _set.ExceptWith(other);
        }

        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            _set.SymmetricExceptWith(other);
        }

        public bool IsSubsetOf(IEnumerable<T> other)
        {
            return _set.IsSubsetOf(other);
        }

        public bool IsSupersetOf(IEnumerable<T> other)
        {
            return _set.IsSupersetOf(other);
        }

        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            return _set.IsProperSupersetOf(other);
        }

        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            return _set.IsProperSubsetOf(other);
        }

        public bool Overlaps(IEnumerable<T> other)
        {
            return _set.Overlaps(other);
        }

        public bool SetEquals(IEnumerable<T> other)
        {
            return _set.SetEquals(other);
        }

        public bool Contains(T item)
        {
            return _set.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _set.CopyTo(array, arrayIndex);
        }

        public void Clear()
        {
            foreach (var element in _set)
                OnRemove(this, element);
            _set.Clear();
        }

        public HashSet<T>.Enumerator GetEnumerator()
        {
            return _set.GetEnumerator();
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
