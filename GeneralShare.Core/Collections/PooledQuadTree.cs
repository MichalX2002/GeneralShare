using System;
using Microsoft.Xna.Framework;

namespace GeneralShare.Collections
{
    public class PooledQuadTree<T> : QuadTree<T>
    {
        private readonly ListArray<ListArray<Item>> _pool;
        private Func<ListArray<Item>> _getListFunc;
        
        public PooledQuadTree(RectangleF boundary, int threshold, bool allowOverflow) :
            base(boundary, threshold, allowOverflow)
        {
            _pool = new ListArray<ListArray<Item>>();
            _getListFunc = GetList;
        }

        public PooledQuadTree(int threshold, bool allowOverflow) :
            this(RectangleF.Empty, threshold, allowOverflow)
        {
        }

        private ListArray<Item> GetList()
        {
            if(_pool.Count > 0)
                return _pool.GetAndRemoveLast();
            return new ListArray<Item>();
        }

        public void ClearPool()
        {
            _pool.Clear();
        }

        public void Resize(RectangleF boundary)
        {
            var tmpList = GetList();

            void AddToTmp(QuadTree<T> tree)
            {
                foreach (var rect in tree.Items)
                    tmpList.Add(rect);
                if (tree.IsDivided)
                {
                    AddToTmp(tree.TopLeft);
                    AddToTmp(tree.TopRight);
                    AddToTmp(tree.BottomLeft);
                    AddToTmp(tree.BottomRight);
                }
            }
            AddToTmp(this);

            void ReturnTreeLists(QuadTree<T> tree)
            {
                ReturnList(tree.Items);
                if (tree.IsDivided)
                {             
                    ReturnTreeLists(tree.TopLeft);
                    ReturnTreeLists(tree.TopRight);
                    ReturnTreeLists(tree.BottomLeft);
                    ReturnTreeLists(tree.BottomRight);
                }
            }
            ReturnTreeLists(this);

            foreach (var item in tmpList)
                Insert(item);
        }

        private void ReturnList(ListArray<Item> list)
        {
            list.Clear();
            _pool.Add(list);
        }
    }
}
