using Microsoft.Xna.Framework;

namespace GeneralShare.Collections
{
    public partial class QuadTree<T>
    {
        private ReadOnlyQuadTree<T> _readonlyTree;

        public RectangleF Bounds { get; private set; }
        public int Threshold { get; set; }
        public bool AllowOverflow { get; set; }
        public bool UseFuzzyBoundaries { get; set; }
        public ListArray<QuadTreeItem<T>> Items { get; }

        public bool IsDivided { get; private set; }
        public QuadTree<T> TopLeft { get; private set; }
        public QuadTree<T> TopRight { get; private set; }
        public QuadTree<T> BottomLeft { get; private set; }
        public QuadTree<T> BottomRight { get; private set; }

        internal QuadTree(RectangleF bounds, int threshold, bool allowOverflow, bool fuzzyBoundaries)
        {
            Items = new ListArray<QuadTreeItem<T>>(threshold);
            Set(bounds, threshold, allowOverflow, fuzzyBoundaries);
        }
        
        internal void Set(RectangleF bounds, int threshold, bool allowOverflow, bool fuzzyBoundaries)
        {
            Bounds = bounds;
            Threshold = threshold;
            AllowOverflow = allowOverflow;
            UseFuzzyBoundaries = fuzzyBoundaries;
        }

        public ReadOnlyQuadTree<T> AsReadOnly()
        {
            if(_readonlyTree == null)
                _readonlyTree = new ReadOnlyQuadTree<T>(this);
            return _readonlyTree;
        }

        /// <summary>
        /// Returns a list containing every item from this <see cref="QuadTree{T}"/>.
        /// </summary>
        /// <returns><see cref="ListArray{QuadTreeItem{T}}"/> containing every item.</returns>
        public ListArray<QuadTreeItem<T>> GetItems()
        {
            var items = QuadTreePool<T>.RentItemList();
            void AddToList(QuadTree<T> tree)
            {
                items.AddRange(tree.Items);
                if (tree.IsDivided)
                {
                    AddToList(tree.TopLeft);
                    AddToList(tree.TopRight);
                    AddToList(tree.BottomLeft);
                    AddToList(tree.BottomRight);
                }
            }
            AddToList(this);
            return items;
        }
    }
}
