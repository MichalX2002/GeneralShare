using Microsoft.Xna.Framework;

namespace GeneralShare.Collections
{
    public partial class QuadTree<T>
    {
        public QuadTreeItem<T>? QueryNearest(RectangleF range, PointF start)
        {
            var items = Query(range);
            if (items == null)
                return null;

            bool hasValue = false;
            float lastItemDst = default;
            QuadTreeItem<T> lastItem = default;
            foreach (var item in items)
            {
                if (hasValue &&
                    item.Bounds.SquaredDistanceTo(start).CompareTo(lastItemDst) >= 0)
                    continue;

                hasValue = true;
                lastItem = item;
                lastItemDst = item.Bounds.SquaredDistanceTo(start);
            }

            QuadTreePool<T>.Return(items);

            if (!hasValue)
                return null;
            return lastItem;
        }
        
        public ListArray<QuadTreeItem<T>> Query(PointF point)
        {
            return Query(new RectangleF(point, SizeF.Empty));
        }

        public ListArray<QuadTreeItem<T>> Query(RectangleF range)
        {
            if (!Bounds.Intersects(range))
                return null;

            var result = QuadTreePool<T>.RentItemList();
            var items = GetItems();

            foreach (var item in items)
                if (range.Intersects(item.Bounds))
                    result.Add(item);

            QuadTreePool<T>.Return(items);
            return result;
        }
    }
}
