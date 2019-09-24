using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace GeneralShare.Collections
{
    public class ReadOnlyQuadTree<T>
    {
        private readonly QuadTree<T> _tree;

        public bool IsDivided => _tree.IsDivided;
        public IReadOnlyList<QuadTreeItem<T>> Items => _tree.Items.AsReadOnly();
        public ReadOnlyQuadTree<T> TopLeft => _tree.TopLeft.AsReadOnly();
        public ReadOnlyQuadTree<T> TopRight => _tree.TopRight.AsReadOnly();
        public ReadOnlyQuadTree<T> BottomLeft => _tree.BottomLeft.AsReadOnly();
        public ReadOnlyQuadTree<T> BottomRight => _tree.BottomRight.AsReadOnly();

        public ReadOnlyQuadTree(QuadTree<T> tree)
        {
            _tree = tree;
        }

        public QuadTreeItem<T>? QueryNearest(RectangleF range, PointF start)
        {
            return _tree.QueryNearest(range, start);
        }

        public ListArray<QuadTreeItem<T>> Query(PointF point)
        {
            return _tree.Query(point);
        }
        
        public ListArray<QuadTreeItem<T>> Query(RectangleF range)
        {
            return _tree.Query(range);
        }

        /// <summary>
        /// Returns a list containing every item from this <see cref="QuadTree{T}"/>.
        /// </summary>
        /// <returns><see cref="ListArray{QuadTreeItem{T}}"/> containing every item.</returns>
        public ListArray<QuadTreeItem<T>> GetItems()
        {
            return _tree.GetItems();
        }
    }
}