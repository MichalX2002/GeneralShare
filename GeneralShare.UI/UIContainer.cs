using GeneralShare.Collections;
using MonoGame.Extended;
using System.Collections.Generic;

namespace GeneralShare.UI
{
    public class UIContainer : UIElement
    {
        private ListArray<UITransform> _transforms;
        private RectangleF _bounds;

        public override RectangleF Boundaries => _bounds;
        public ReadOnlyWrapper<UITransform> Children { get; }
        
        public UIContainer(UIManager manager) : base(manager)
        {
            _transforms = new ListArray<UITransform>();
            Children = new ReadOnlyWrapper<UITransform>(_transforms);
        }

        public UIContainer() : this(null)
        {
        }

        private void Transform_MarkedDirty(DirtMarkType marks)
        {
            if (marks.HasFlags(DirtMarkType.Boundaries))
                UpdateBounds();
        }

        private void AddInternal(UITransform transform)
        {
            _transforms.Add(transform);
            transform.MarkedDirty += Transform_MarkedDirty;
        }

        public void Add(UITransform transform)
        {
            lock (SyncRoot)
            {
                AddInternal(transform);
                UpdateBounds();
            }
        }

        public void AddRange(IEnumerable<UITransform> transforms)
        {
            lock (SyncRoot)
            {
                foreach (var item in transforms)
                    AddInternal(item);
                UpdateBounds();
            }
        }

        private bool RemoveInternal(UITransform original, UITransform candidate, int i)
        {
            original.MarkedDirty -= Transform_MarkedDirty;
            if (candidate.TransformKey == original.TransformKey)
            {
                _transforms.RemoveAt(i);
                return true;
            }
            return false;
        }

        public bool Remove(UITransform transform)
        {
            lock (SyncRoot)
            {
                for (int i = 0, count = _transforms.Count; i < count; i++)
                {
                    if (RemoveInternal(transform, _transforms[i], i))
                        return true;
                }
                UpdateBounds();
                return false;
            }
        }

        public int RemoveRange(IEnumerable<UITransform> transforms)
        {
            lock (SyncRoot)
            {
                int items = 0;
                foreach (var original in transforms)
                {
                    for (int i = _transforms.Count; i-- > 0;)
                    {
                        if (RemoveInternal(original, _transforms[i], i))
                        {
                            items += 1;
                        }
                    }
                }
                UpdateBounds();
                return items;
            }
        }

        private void UpdateBounds()
        {
            _bounds = RectangleF.Empty;
            for (int i = 0, count = _transforms.Count; i < count; i++)
            {
                var transform = _transforms[i];
                if (transform is UIElement element && transform.Enabled)
                    RectangleF.Union(_bounds, element.Boundaries, out _bounds);
            }
            InvokeMarkedDirty(DirtMarkType.Boundaries);
        }

        protected override void Dispose(bool disposing)
        {
            if (!Disposed)
            {
                for (int i = 0, count = _transforms.Count; i < count; i++)
                {
                    _transforms[i].MarkedDirty -= Transform_MarkedDirty;
                }
                _transforms = null;

                base.Dispose(disposing);
            }
        }
    }
}
