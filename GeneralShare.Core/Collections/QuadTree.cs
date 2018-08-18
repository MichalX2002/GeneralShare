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

        public RectangleF Boundary { get; private set; }
        public int Threshold { get; private set; }
        public ListArray<Item> Items { get; private set; }
        public bool AllowOverflow { get; private set; }

        public bool Divided { get; private set; }
        public QuadTree<T> TopLeft { get; private set; }
        public QuadTree<T> TopRight { get; private set; }
        public QuadTree<T> BottomLeft { get; private set; }
        public QuadTree<T> BottomRight { get; private set; }

        public QuadTree(RectangleF boundary, int threshold, bool allowOverflow,
            Func<ListArray<Item>> getListFunc)
        {
            Boundary = boundary;
            Threshold = threshold;
            AllowOverflow = allowOverflow;
            _getListFunc = getListFunc ?? throw new ArgumentNullException(nameof(getListFunc));
            Items = _getListFunc();
        }

        public QuadTree(RectangleF boundary, int threshold, bool allowOverflow)
        {
            Boundary = boundary;
            Threshold = threshold;
            AllowOverflow = allowOverflow;
            _getListFunc = () => new ListArray<Item>(Threshold);
            Items = _getListFunc();
        }

        public QuadTree(RectangleF boundary, int threshold, bool allowOverflow,
            ListArray<Item> existingList)
        {
            Boundary = boundary;
            Threshold = threshold;
            AllowOverflow = allowOverflow;
            _getListFunc = () => new ListArray<Item>(Threshold);
            Items = existingList;
        }

        public QuadTree(float x, float y, float width, float height,
            int threshold, bool allowOverflow, ListArray<Item> existingList) :
            this(new RectangleF(x, y, width, height), threshold, allowOverflow, existingList)
        {
        }
        
        public ReadOnlyQuadTree<T> AsReadOnly()
        {
            if(_readonlyTree == null)
                _readonlyTree = new ReadOnlyQuadTree<T>(this);
            return _readonlyTree;
        }
        
        public Item QueryNearest(RectangleF range, Vector2 start)
        {
            return QueryNearest(range, (Point2)start);
        }

        public Item QueryNearest(RectangleF range, Point2 start)
        {
            return Query(range).FirstMin(item => item.Bounds.SquaredDistanceTo(start));
        }

        public IEnumerable<Item> Query(Vector2 point)
        {
            return Query((Point2)point);
        }

        public IEnumerable<Item> Query(Point2 point)
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
            return Insert((Point2)item, value);
        }

        public bool Insert(Point2 item, T value)
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
                    Subdivide(_getListFunc);

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
                Items.Add(in item);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Retreives all items in this <see cref="QuadTree"/> 
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
        /// Retreives all the lists in this <see cref="QuadTree"/> 
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

        private void Subdivide(Func<ListArray<Item>> getListFunc)
        {
            float x = Boundary.X;
            float y = Boundary.Y;
            float w = Boundary.Width / 2f;
            float h = Boundary.Height / 2f;
            int t = Items.Capacity;

            TopLeft = new QuadTree<T>(x, y, w, h, t, AllowOverflow, getListFunc());
            TopRight = new QuadTree<T>(x + w, y, w, h, t, AllowOverflow, getListFunc());
            BottomLeft = new QuadTree<T>(x, y + h, w, h, t, AllowOverflow, getListFunc());
            BottomRight = new QuadTree<T>(x + w, y + h, w, h, t, AllowOverflow, getListFunc());

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
