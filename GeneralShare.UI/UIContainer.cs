using GeneralShare.Collections;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using System.Collections.Generic;

namespace GeneralShare.UI
{
    public class UIContainer : UIElement
    {
        private ListArray<UITransform> _transforms;
        private RectangleF _bounds;
        private bool _needsBoundsUpdate;

        public override RectangleF Boundaries => GetBounds();
        public ReadOnlyWrapper<UITransform> Children { get; }
        
        public UIContainer(UIManager manager) : base(manager)
        {
            _transforms = new ListArray<UITransform>();
            Children = new ReadOnlyWrapper<UITransform>(_transforms);
            InterceptCursor = false;
        }

        public UIContainer() : this(null)
        {
        }

        private void Transform_MarkedDirty(DirtMarkType marks)
        {
            if (marks.HasAnyFlag(DirtMarkType.Boundaries, DirtMarkType.Enabled))
                _needsBoundsUpdate = true;
        }

        private void AddInternal(UITransform transform)
        {
            _transforms.Add(transform);
            lock (transform.SyncRoot)
            {
                transform.SetContainer(this);
                transform.MarkedDirty += Transform_MarkedDirty;
            }
        }

        public void Add(UITransform transform)
        {
            lock (SyncRoot)
            {
                AddInternal(transform);
                _needsBoundsUpdate = true;
            }
        }

        public void AddRange(IEnumerable<UITransform> transforms)
        {
            lock (SyncRoot)
            {
                foreach (var item in transforms)
                {
                    AddInternal(item);
                }
                _needsBoundsUpdate = true;
            }
        }

        private bool RemoveInternal(UITransform original, UITransform candidate, int i)
        {
            lock (original.SyncRoot)
            {
                if (original.Container == this)
                    original.SetContainer(null);

                original.MarkedDirty -= Transform_MarkedDirty;
            }
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
                _needsBoundsUpdate = true;
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
                _needsBoundsUpdate = true;
                return items;
            }
        }
        
        private RectangleF GetBounds()
        {
            if (_needsBoundsUpdate)
            {
                UpdateBounds();
                _needsBoundsUpdate = false;
            }
            return _bounds;
        }

        private void UpdateBounds()
        {
            lock (SyncRoot)
            {
                bool hasOriginRect = false;
                for (int i = 0, count = _transforms.Count; i < count; i++)
                {
                    var transform = _transforms[i];
                    if (transform is UIElement element)
                    {
                        lock (element.SyncRoot)
                        {
                            if (element.IsEnabled)
                            {
                                if (hasOriginRect)
                                    RectangleF.Union(_bounds, element.Boundaries, out _bounds);
                                else
                                {
                                    _bounds = element.Boundaries;
                                    hasOriginRect = true;
                                }
                            }
                        }
                    }
                }
            }
            InvokeMarkedDirty(DirtMarkType.Boundaries);
        }

        public override void Update(GameTime time)
        {
            if (Dirty)
            {
                lock (SyncRoot)
                {
                    for (int i = 0, count = _transforms.Count; i < count; i++)
                    {
                        _transforms[i].InvokeMarkedDirtyInternal(UITransform.FULL_TRANSFORM_UPDATE);
                    }
                }
                Dirty = false;
                ClearDirtMarks();
            }
            base.Update(time);
        }

        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed)
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
