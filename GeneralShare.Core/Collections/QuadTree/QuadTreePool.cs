using Microsoft.Xna.Framework;
using MonoGame.Extended.Collections;

namespace GeneralShare.Collections
{
    public static class QuadTreePool<T>
    {
        private static Bag<QuadTree<T>> _treePool;
        private static Bag<ListArray<QuadTreeItem<T>>> _listPool;

        static QuadTreePool()
        {
            _treePool = new Bag<QuadTree<T>>();
            _listPool = new Bag<ListArray<QuadTreeItem<T>>>();
        }

        public static QuadTree<T> Rent(RectangleF boundary, int threshold, bool allowOverflow, bool fuzzyBoundaries)
        {
            if (_treePool.TryTake(out var tree))
            {
                tree.Set(boundary, threshold, allowOverflow, fuzzyBoundaries);
                return tree;
            }
            return new QuadTree<T>(boundary, threshold, allowOverflow, fuzzyBoundaries);
        }

        public static QuadTree<T> Rent(
            float x, float y, float width, float height, int threshold, bool allowOverflow, bool fuzzyBoundaries)
        {
            return Rent(new RectangleF(x, y, width, height), threshold, allowOverflow, fuzzyBoundaries);
        }

        public static ListArray<QuadTreeItem<T>> RentItemList()
        {
            if (_listPool.TryTake(out var list))
                return list;
            return new ListArray<QuadTreeItem<T>>();
        }

        public static void Return(ListArray<QuadTreeItem<T>> list)
        {
            // clear as it may contain reference-types
            list.Clear();
            _listPool.Add(list);
        }

        public static void Return(QuadTree<T> tree)
        {
            // clear now as it may contain reference-types
            tree.Clear();
            _treePool.Add(tree);
        }
    }
}