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
        public IReadOnlyList<UITransform> Children { get; }
        
        public UIContainer(UIManager manager) : base(manager)
        {
            _transforms = new ListArray<UITransform>();
            Children = new ReadOnlyWrapper<UITransform>(_transforms);
            IsIntercepting = false;
            IsDrawable = false;
        }

        private void Transform_MarkedDirty(UITransform transform, DirtMarkType marks)
        {
            if (marks.HasAnyFlag(DirtMarkType.Boundaries, DirtMarkType.Enabled))
                _needsBoundsUpdate = true;
        }
        
        public void Add(UITransform transform)
        {
            _transforms.Add(transform);
            transform.Container = null;
            transform.MarkedDirty += Transform_MarkedDirty;
            _needsBoundsUpdate = true;
        }

        public void AddRange(IEnumerable<UITransform> transforms)
        {
            foreach (var transform in transforms)
                Add(transform);
        }
        
        public bool Remove(UITransform transform)
        {
            if (transform.Container == this)
                transform.Container = null;
            transform.MarkedDirty -= Transform_MarkedDirty;

            _transforms.Remove(transform);
            _needsBoundsUpdate = true;
            return false;
        }

        public int RemoveRange(IEnumerable<UITransform> transforms)
        {
            int count = 0;
            foreach (var candidate in transforms)
                if (Remove(candidate))
                    count++;
            return count;
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
            bool hasOriginRect = false;
            for (int i = 0; i < _transforms.Count; i++)
            {
                var transform = _transforms[i];
                if (transform is UIElement element && element.IsEnabled)
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
            MarkDirty(DirtMarkType.Boundaries);
        }

        public override void Update(GameTime time)
        {
            if (IsDirty == false)
                return;

            DirtMarkType marks = FULL_TRANSFORM_UPDATE;
            if (HasAnyDirtMark(DirtMarkType.Enabled))
                marks |= DirtMarkType.Enabled;

            for (int i = 0; i < _transforms.Count; i++)
                _transforms[i].InvokeMarkedDirty(marks);

            MarkClean();
        }

        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                for (int i = 0; i < _transforms.Count; i++)
                    _transforms[i].MarkedDirty -= Transform_MarkedDirty;
                _transforms = null;

                base.Dispose(disposing);
            }
        }
    }
}
