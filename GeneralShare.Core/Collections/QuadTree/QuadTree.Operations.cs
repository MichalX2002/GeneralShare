using Microsoft.Xna.Framework;

namespace GeneralShare.Collections
{
    public partial class QuadTree<T>
    {   
        public void Clear()
        {
            void RecursiveTreeReturn(QuadTree<T> tree, bool isRoot = false)
            {
                if (tree.IsDivided)
                {
                    RecursiveTreeReturn(tree.TopLeft);
                    RecursiveTreeReturn(tree.TopRight);
                    RecursiveTreeReturn(tree.BottomLeft);
                    RecursiveTreeReturn(tree.BottomRight);
                }
                tree.TopLeft = null;
                tree.TopRight = null;
                tree.BottomLeft = null;
                tree.BottomRight = null;
                tree.IsDivided = false;

                if (!isRoot)
                    QuadTreePool<T>.Return(tree);
            }

            RecursiveTreeReturn(this, isRoot: true);
            Items.Clear();
        }
        
        public void Resize(RectangleF boundary)
        {
            Bounds = boundary;
            var items = GetItems();

            Clear();

            foreach (var item in items)
                Insert(item);

            QuadTreePool<T>.Return(items);
        }

        private void Subdivide()
        {
            float x = Bounds.X;
            float y = Bounds.Y;
            float w = Bounds.Width / 2f;
            float h = Bounds.Height / 2f;

            // we cannot allow fuzzyBoundaries for leaf trees as that breaks precision
            TopLeft = QuadTreePool<T>.Rent(x, y, w, h, Threshold, AllowOverflow, fuzzyBoundaries: false);
            TopRight = QuadTreePool<T>.Rent(x + w, y, w, h, Threshold, AllowOverflow, fuzzyBoundaries: false);
            BottomLeft = QuadTreePool<T>.Rent(x, y + h, w, h, Threshold, AllowOverflow, fuzzyBoundaries: false);
            BottomRight = QuadTreePool<T>.Rent(x + w, y + h, w, h, Threshold, AllowOverflow, fuzzyBoundaries: false);

            IsDivided = true;
        }
    }
}
