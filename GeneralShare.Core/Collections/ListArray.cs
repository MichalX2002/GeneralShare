using MonoGame.Extended.Collections;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace GeneralShare.Collections
{
    public class ListArray<T> :
        ICollection<T>, IList<T>, IReadOnlyCollection<T>,
        IReadOnlyList<T>, IEnumerable<T>, IReferenceList<T>
    {
        public delegate void VersionChangedDelegate(int oldVersion, int newVersion);
        public event VersionChangedDelegate Changed;
        
        private const int _defaultCapacity = 4;

        private ReadOnlyCollection<T> _readonly;
        private T[] _innerArray;
        private int _size;
        private int __version;

        public bool IsReadOnly { get; private set; }
        public bool IsFixedCapacity { get; private set; }
        public int Version => __version;

        public T[] InnerArray => _innerArray;
        public int Count => _size;

        public T this[int index]
        {
            get
            {
                if (index >= _size)
                    throw new ArgumentOutOfRangeException(nameof(index));

                return _innerArray[index];
            }
            set
            {
                CheckAccessibility();
                _innerArray[index] = value;
                UpdateVersion();
            }
        }

        public int Capacity
        {
            get => _innerArray.Length;
            set
            {
                if (IsFixedCapacity)
                    throw new InvalidOperationException(
                        "This collection has a fixed capacity therefore cannot be resized.");

                CheckAccessibility();

                if (value != _innerArray.Length)
                {
                    if (value < _size)
                        throw new ArgumentException(
                            "The new capacity is not enough to contain existing items.", nameof(value));

                    if (value > 0)
                    {
                        T[] newItems = new T[value];
                        if (_size > 0)
                            Array.Copy(_innerArray, 0, newItems, 0, _size);
                        _innerArray = newItems;
                    }
                    else
                        _innerArray = Array.Empty<T>();

                    UpdateVersion();
                }
            }
        }

        public ListArray()
        {
            _innerArray = Array.Empty<T>();
        }

        public ListArray(int capacity)
        {
            _innerArray = new T[capacity];
        }

        public ListArray(int capacity, bool fixedCapacity) : this(capacity)
        {
            IsFixedCapacity = fixedCapacity;
        }

        public ListArray(T[] sourceArray, int startOffset, int count)
        {
            _innerArray = sourceArray;
            _size = count;
            Capacity = sourceArray.Length;
            IsFixedCapacity = true;

            if (startOffset != 0)
            {
                Array.ConstrainedCopy(sourceArray, startOffset, sourceArray, 0, count);
                Array.Clear(sourceArray, count, Capacity - count);
            }
        }

        public ListArray(T[] sourceArray, int count) : this(sourceArray, 0, count)
        {
        }

        public ListArray(T[] sourceArray) : this(sourceArray, 0, sourceArray.Length)
        {
        }

        public ListArray(IEnumerable<T> collection, bool readOnly)
        {
            if (collection is ICollection<T> c)
            {
                int count = c.Count;
                if (count != 0)
                {
                    _innerArray = new T[count];
                    c.CopyTo(_innerArray, 0);
                    _size = count;
                }
                else
                    _innerArray = Array.Empty<T>();
            }
            else
            {
                _size = 0;
                _innerArray = Array.Empty<T>();
                AddRange(collection);
            }

            IsReadOnly = readOnly;
        }
        
        public ListArray(IEnumerable<T> collection) : this(collection, false)
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void UpdateVersion()
        {
            int newVersion = unchecked(__version + 1);
            Changed?.Invoke(__version, newVersion);
            __version = newVersion;
        }
        
        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CheckAccessibility()
        {
            if (IsReadOnly)
                throw new InvalidOperationException("This collection is marked as read-only.");
        }

        public ref T GetReferenceAt(int index)
        {
            if (index >= _size)
                throw new IndexOutOfRangeException();

            return ref _innerArray[index];
        }
        
        public void Add(T item)
        {
            AddCheck();
            _innerArray[_size++] = item;
            UpdateVersion();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AddCheck()
        {
            CheckAccessibility();

            if (_size == _innerArray.Length)
                EnsureCapacity(_size + 1);
        }

        public void AddRange(IEnumerable<T> collection)
        {
            InsertRange(_size, collection);
        }

        public void Sort(IComparer<T> comparer)
        {
            Sort(0, _size, comparer);
        }

        public void Sort(int index, int count, IComparer<T> comparer)
        {
            CheckAccessibility();

            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index));

            if (count < 0)
                throw new ArgumentOutOfRangeException(
                    nameof(count), "Needs a non-negative number.");

            if (_size - index < count)
                throw new ArgumentException("Invalid offset length.");

            Array.Sort(_innerArray, index, count, comparer);
            UpdateVersion();
        }
        
        public void Clear()
        {
            CheckAccessibility();

            if (_size > 0)
            {
                Array.Clear(_innerArray, 0, _size);

                _size = 0;
                UpdateVersion();
            }
        }

        public bool Contains(T item)
        {
            return IndexOf(item) != -1;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            Array.ConstrainedCopy(_innerArray, 0, array, arrayIndex, _size);
        }

        public bool Remove(T item)
        {
            int index = IndexOf(item);
            if (index != -1)
            {
                RemoveAtInternal(index);
                return true;
            }
            return false;
        }
        
        public void RemoveAt(int index)
        {
            GetAndRemoveAt(index);
        }

        public T GetAndRemoveAt(int index)
        {
            if (index >= _size || index < 0)
                throw new IndexOutOfRangeException();

            return RemoveAtInternal(index);
        }

        public T GetAndRemoveLast()
        {
            if (_size > 0)
                return GetAndRemoveAt(_size - 1);
            return default;
        }

        public void RemoveRange(int index, int count)
        {
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index));

            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(index));

            if (_size - index < count)
                throw new ArgumentException(
                    $"{nameof(index)} and {nameof(count)} do not denote a valid range of elements.");

            CheckAccessibility();

            if (count > 0)
            {
                _size -= count;
                if (index < _size)
                    Array.Copy(_innerArray, index + count, _innerArray, index, _size - index);

                Array.Clear(_innerArray, _size, count);
                UpdateVersion();
            }
        }
        
        private T RemoveAtInternal(int index)
        {
            CheckAccessibility();

            _size--;
            if (index < _size)
                Array.Copy(_innerArray, index + 1, _innerArray, index, _size - index);

            T item = _innerArray[_size];
            _innerArray[_size] = default;
            UpdateVersion();
            return item;
        }

        public int FindIndex(Predicate<T> predicate)
        {
            for (int i = 0; i < _size; i++)
            {
                if (predicate.Invoke(_innerArray[i]) == true)
                    return i;
            }
            return -1;
        }

        public int IndexOf(T item)
        {
            return Array.IndexOf(_innerArray, item, 0, _size);
        }

        public void Insert(int index, T item)
        {
            CheckAccessibility();

            if (index > Capacity)
                throw new IndexOutOfRangeException();

            if (_size == _innerArray.Length)
                EnsureCapacity(_size + 1);

            if (index < _size)
                Array.Copy(_innerArray, index, _innerArray, index + 1, _size - index);

            _innerArray[index] = item;
            _size++;
            UpdateVersion();
        }

        public void InsertRange(int index, IEnumerable<T> collection)
        {
            if (index > _size)
                throw new ArgumentOutOfRangeException(nameof(index));

            if (collection == null)
                throw new ArgumentNullException(nameof(collection));

            CheckAccessibility();
            
            if (collection is ICollection<T> c)
            {
                int count = c.Count;
                if (count > 0)
                {
                    EnsureCapacity(_size + count);
                    if (index < _size)
                        Array.Copy(_innerArray, index, _innerArray, index + count, count - index);

                    if (c == this)
                    {
                        Array.Copy(_innerArray, 0, _innerArray, index, index);
                        Array.Copy(_innerArray, index + count, _innerArray, index * 2, _size - index);
                    }
                    else
                        c.CopyTo(_innerArray, index);

                    _size += count;
                }
            }
            else
            {
                foreach (var item in collection)
                    Insert(index++, item);
            }
        }

        public void InsertRange(int index, int count, T item)
        {
            if (index > _size)
                throw new ArgumentOutOfRangeException(nameof(index));

            CheckAccessibility();

            if (count > 0)
            {
                EnsureCapacity(_size + count);
                if (index < _size)
                    Array.Copy(_innerArray, index, _innerArray, index + count, count - index);

                for (int i = 0; i < count; i++)
                    _innerArray[index + i] = item;

                _size += count;
            }
        }

        public ReadOnlyCollection<T> AsReadOnly()
        {
            if (_readonly == null)
                _readonly = new ReadOnlyCollection<T>(this);
            return _readonly;
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private void EnsureCapacity(int min)
        {
            if (_innerArray.Length < min)
            {
                int newCapacity = _innerArray.Length == 0 ? _defaultCapacity : _innerArray.Length * 2;
                if (newCapacity < min)
                    newCapacity = min;

                Capacity = newCapacity;
            }
        }

        public struct Enumerator : IEnumerator<T>, IEnumerator
        {
            private ListArray<T> _list;
            private int _index;
            private readonly int _version;

            public T Current { get; private set; }

            object IEnumerator.Current
            {
                get
                {
                    if (_index == 0 || _index == _list._size + 1)
                    {
                        throw new InvalidOperationException(
                            "Either MoveNext has not been called or index is beyond item count.");
                    }
                    return Current;
                }
            }

            public Enumerator(ListArray<T> list)
            {
                _list = list;
                _index = 0;
                _version = _list.__version;
                Current = default;
            }

            public bool MoveNext()
            {
                if (_version == _list.__version && _index < _list._size)
                {
                    Current = _list._innerArray[_index];
                    _index++;
                    return true;
                }
                return MoveNextRare();
            }

            private bool MoveNextRare()
            {
                if (_version != _list.__version)
                {
                    throw GetVersionException();
                }

                _index = _list._size + 1;
                Current = default;
                return false;
            }

            void IEnumerator.Reset()
            {
                if (_version != _list.__version)
                {
                    throw GetVersionException();
                }

                _index = 0;
                Current = default;
            }

            private InvalidOperationException GetVersionException()
            {
                return new InvalidOperationException(
                    "The underlying list version has changed.");
            }

            public void Dispose()
            {
            }
        }
    }
}
