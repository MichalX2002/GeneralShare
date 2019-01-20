using Microsoft.Xna.Framework;

namespace GeneralShare.Collections
{
    public partial class QuadTree<T>
    {
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
            return Insert(new QuadTreeItem<T>(bounds, value));
        }

        public bool Insert(QuadTreeItem<T> item)
        {
            if (UseFuzzyBoundaries)
            {
                if (!Bounds.Intersects(item.Bounds))
                    return false;
            }
            else
            {
                if (!Bounds.Contains(item.Bounds))
                    return false;
            }

            if (Items.Count >= Threshold)
            {
                if (!IsDivided)
                    Subdivide();

                if (TopLeft.Insert(item) ||
                    TopRight.Insert(item) ||
                    BottomLeft.Insert(item) || 
                    BottomRight.Insert(item))
                    return true;
            }
            else
            {
                Items.Add(item);
                return true;
            }

            if (AllowOverflow)
            {
                Items.Add(item);
                return true;
            }

            return false;
        }
    }
}
