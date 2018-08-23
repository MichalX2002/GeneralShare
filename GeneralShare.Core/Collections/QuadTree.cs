using Microsoft.Xna.Framework;
using MonoGame.Extended;
using System;
using System.Collections.Generic;

namespace GeneralShare.Collections
{
    public class QuadTree<T>
    {
        private ReadOnlyQuadTree<T> _readonlyTree;
        private readonly Func<ListArray<Item>> _getListFunc;

        public RectangleF Boundary { get; }
        public int Threshold { get; }
        public ListArray<Item> Items { get; }
        public bool AllowOverflow { get;  }

        public bool Divided { get; private set; }
        public QuadTree<T> TopLeft { get; private set; }
        public QuadTree<T> TopRight { get; private set; }
        public QuadTree<T> BottomLeft { get; private set; }
        public QuadTree<T> BottomRight { get; private set; }

        public QuadTree(
            RectangleF boundary, int threshold, bool allowOverflow,
            ListArray<Item> items, Func<ListArray<Item>> getListFunc)
        {
            Boundary = boundary;
            Threshold = threshold;
            AllowOverflow = allowOverflow;
            _getListFunc = getListFunc ?? throw new ArgumentNullException(nameof(getListFunc));
            Items = items;
        }

        public QuadTree(
            RectangleF boundary, int threshold, bool allowOverflow, Func<ListArray<Item>> getListFunc) :
            this(boundary, threshold, allowOverflow, getListFunc(), getListFunc)
        {
        }

        public QuadTree(RectangleF boundary, int threshold, bool allowOverflow) :
            this(boundary, threshold, allowOverflow, DefaultGetList(threshold), () => DefaultGetList(threshold))
        {
        }

        public QuadTree(
            RectangleF boundary, int threshold, bool allowOverflow, ListArray<Item> existingList) :
            this(boundary, threshold, allowOverflow, existingList, () => DefaultGetList(threshold))
        {
        }

        public QuadTree(
            float x, float y, float width, float height,
            int threshold, bool allowOverflow, ListArray<Item> existingList) :
            this(new RectangleF(x, y, width, height), threshold, allowOverflow, existingList)
        {
        }
        
        private static ListArray<Item> DefaultGetList(int threshold)
        {
            return new ListArray<Item>(threshold);
        }

        public ReadOnlyQuadTree<T> AsReadOnly()
        {
            if(_readonlyTree == null)
                _readonlyTree = new ReadOnlyQuadTree<T>(this);
            return _readonlyTree;
        }
        
        public Item QueryNearest(RectangleF range, Vector2 start)
        {
            return QueryNearest(range, (PointF)start);
        }

        public Item QueryNearest(RectangleF range, PointF start)
        {
            return Query(range).FirstMin(item => item.Bounds.SquaredDistanceTo(start));
        }

        public IEnumerable<Item> Query(Vector2 point)
        {
            return Query((PointF)point);
        }

        public IEnumerable<Item> Query(PointF point)
        {
            return Query(new RectangleF(point, SizeF.Empty));
        }

        public IEnumerable<Item> Query(RectangleF range)
        {
            if (Boundary.Intersects(range) == false)
                yield break;

            foreach (var item in EnumerateItems())
                if (range.Contains(item.Bounds))
                    yield return item;
        }

        public bool Insert(Vector2 item, T value)
        {
            return Insert((PointF)item, value);
        }

        public bool Insert(PointF item, T value)
        {
            return Insert(new RectangleF(item, SizeF.Empty), value);
        }

        public bool Insert(RectangleF bounds, T value)
        {
            return Insert(new Item(bounds, value));
        }

        public bool Insert(in Item item)
        {
            if (Boundary.Contains(item.Bounds) == false)
                return false;

            if (Items.Count < Threshold)
            {
                Items.Add(new Item(item.Bounds, item.Value));
                return true;
            }
            else
            {
                if (Divided == false)
                    Subdivide();

                if (TopLeft.Insert(item) == true)
                    return true;
                if (TopRight.Insert(item) == true)
                    return true;
                if (BottomLeft.Insert(item) == true)
                    return true;
                if (BottomRight.Insert(item) == true)
                    return true;
            }

            if(AllowOverflow)
            {
                Items.Add(item);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Retreives all items in this <see cref="QuadTree{T}"/> 
        /// including items from the <see cref="Items"/> list.
        /// </summary>
        /// <returns>Retreives all items from this instance.</returns>
        public IEnumerable<Item> EnumerateItems()
        {
            foreach (var rect in Items)
                yield return rect;

            if (Divided)
            {
                foreach (var rect in TopLeft.EnumerateItems())
                    yield return rect;

                foreach (var rect in TopRight.EnumerateItems())
                    yield return rect;

                foreach (var rect in BottomLeft.EnumerateItems())
                    yield return rect;

                foreach (var rect in BottomRight.EnumerateItems())
                    yield return rect;
            }
        }

        /// <summary>
        /// Retreives all the lists in this <see cref="QuadTree{T}"/> 
        /// including the <see cref="Items"/> list.
        /// </summary>
        /// <returns>Retreives all lists from this instance.</returns>
        public IEnumerable<ListArray<Item>> EnumerateLists()
        {
            yield return Items;

            if (Divided)
            {
                foreach (var list in TopLeft.EnumerateLists())
                    yield return list;

                foreach (var list in TopRight.EnumerateLists())
                    yield return list;

                foreach (var list in BottomLeft.EnumerateLists())
                    yield return list;

                foreach (var list in BottomRight.EnumerateLists())
                    yield return list;
            }
        }

        /// <summary>
        /// Iterates over <see cref="EnumerateLists"/>;
        /// clearing every list before yielding it.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ListArray<Item>> EnumerateClearedLists()
        {
            foreach (var list in EnumerateLists())
            {
                list.Clear();
                yield return list;
            }
        }

        public void Clear()
        {
            foreach (var list in EnumerateLists())
                list.Clear();
        }

        private void Subdivide()
        {
            float x = Boundary.X;
            float y = Boundary.Y;
            float w = Boundary.Width / 2f;
            float h = Boundary.Height / 2f;
            int t = Items.Capacity;

            TopLeft = new QuadTree<T>(x, y, w, h, t, AllowOverflow, _getListFunc());
            TopRight = new QuadTree<T>(x + w, y, w, h, t, AllowOverflow, _getListFunc());
            BottomLeft = new QuadTree<T>(x, y + h, w, h, t, AllowOverflow, _getListFunc());
            BottomRight = new QuadTree<T>(x + w, y + h, w, h, t, AllowOverflow, _getListFunc());

            Divided = true;
        }

        public class Item
        {
            public RectangleF Bounds;
            public T Value;

            internal Item(in RectangleF bounds, in T value)
            {
                Bounds = bounds;
                Value = value;
            }
        }
    }
}
