using MonoGame.Extended;

namespace GeneralShare.Collections
{
    public class PooledQuadTree<T>
    {
        private readonly ListArray<ListArray<QuadTree<T>.Item>> _pool;

        public int Count => _pool.Count;
        public QuadTree<T> CurrentTree { get; private set; }

        public PooledQuadTree(RectangleF boundary, int threshold, bool allowOverflow)
        {
            _pool = new ListArray<ListArray<QuadTree<T>.Item>>();

            CurrentTree = new QuadTree<T>(boundary, threshold, allowOverflow, GetList);
        }

        private ListArray<QuadTree<T>.Item> GetList()
        {
            if(_pool.Count > 0)
            {
                int index = _pool.Count - 1;
                var item = _pool[index];
                _pool.RemoveAt(index);
                return item;
            }

            return new ListArray<QuadTree<T>.Item>();
        }

        public void ClearPool()
        {
            _pool.Clear();
        }

        public void ClearTree()
        {
            CurrentTree.Clear();
        }

        public void Resize(RectangleF boundary, int threshold, bool allowOverflow)
        {
            var oldTree = CurrentTree;
            CurrentTree = new QuadTree<T>(boundary, threshold, allowOverflow, GetList);

            foreach (var item in oldTree.EnumerateItems())
                CurrentTree.Insert(item);

            foreach (var list in oldTree.EnumerateClearedLists())
                _pool.Add(list);
        }

        public void Resize(RectangleF boundary)
        {
            Resize(boundary, CurrentTree.Threshold, CurrentTree.AllowOverflow);
        }
    }
}
