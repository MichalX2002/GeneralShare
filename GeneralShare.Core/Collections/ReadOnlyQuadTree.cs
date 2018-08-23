using Microsoft.Xna.Framework;
using MonoGame.Extended;
using System.Collections.Generic;

namespace GeneralShare.Collections
{
    public class ReadOnlyQuadTree<T>
    {
        private readonly QuadTree<T> _tree;

        public bool Divided => _tree.Divided;
        public IReadOnlyList<QuadTree<T>.Item> Items => _tree.Items.AsReadOnly();
        public ReadOnlyQuadTree<T> TopLeft => _tree.TopLeft.AsReadOnly();
        public ReadOnlyQuadTree<T> TopRight => _tree.TopRight.AsReadOnly();
        public ReadOnlyQuadTree<T> BottomLeft => _tree.BottomLeft.AsReadOnly();
        public ReadOnlyQuadTree<T> BottomRight => _tree.BottomRight.AsReadOnly();

        public ReadOnlyQuadTree(QuadTree<T> tree)
        {
            _tree = tree;
        }

        public QuadTree<T>.Item QueryNearest(RectangleF range, PointF start)
        {
            return _tree.QueryNearest(range, start);
        }

        public IEnumerable<QuadTree<T>.Item> Query(PointF point)
        {
            return _tree.Query(point);
        }
        
        public IEnumerable<QuadTree<T>.Item> Query(RectangleF range)
        {
            return _tree.Query(range);
        }

        public IEnumerable<QuadTree<T>.Item> EnumerateRectangles()
        {
            return _tree.EnumerateItems();
        }

        public IEnumerable<IReadOnlyList<QuadTree<T>.Item>> EnumerateLists()
        {
            foreach (var list in _tree.EnumerateLists())
                yield return list.AsReadOnly();
        }
    }
}