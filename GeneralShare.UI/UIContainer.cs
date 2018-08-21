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

        private void AddInternal(UITransform transform)
        {
            _transforms.Add(transform);
            transform.MarkedDirty += Transform_MarkedDirty;
        }

        private void Transform_MarkedDirty(DirtMarkType type)
        {
            if (type.HasFlags(DirtMarkType.Boundaries))
                UpdateBounds();
        }

        public void Add(UITransform transform)
        {
            AddInternal(transform);
            UpdateBounds();
        }

        public void AddRange(IEnumerable<UITransform> transforms)
        {
            _transforms.AddRange(transforms);
            UpdateBounds();
        }

        public bool Remove(UITransform transform)
        {
            for (int i = 0, count = _transforms.Count; i < count; i++)
            {
                if (RemoveInternal(transform, _transforms[i], i))
                    return true;
            }
            UpdateBounds();
            return false;
        }

        private bool RemoveInternal(UITransform original, UITransform transform, int i)
        {
            if (transform.TransformKey == original.TransformKey)
            {
                _transforms.RemoveAt(i);
                return true;
            }
            return false;
        }

        public int RemoveRange(IEnumerable<UITransform> transforms)
        {
            int items = 0;
            int count = _transforms.Count;
            foreach (var original in transforms)
            {
                for (int i = 0; i < count; i++)
                {
                    if (RemoveInternal(original, _transforms[i], i))
                    {
                        items += 1;
                        count--;
                    }
                }
            }
            UpdateBounds();
            return items;
        }

        public void UpdateBounds()
        {
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

            base.Dispose(disposing);
        }
    }
}
