using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace GeneralShare.UI
{
    public class UITransform : IDisposable
    {
        internal const DirtMarkType FULL_TRANSFORM_UPDATE =
            DirtMarkType.Transform | DirtMarkType.Position |
            DirtMarkType.Origin | DirtMarkType.Scale | DirtMarkType.Rotation;

        private static int _lastTransformKey;

        public delegate void MarkedDirtyDelegate(UITransform sender, DirtMarkType marks);
        public delegate void MarkedCleanDelegate(UITransform sender);

        public event MarkedDirtyDelegate MarkedDirty;
        public event MarkedCleanDelegate MarkedClean;

        private bool _updateViewportOnEnable;
        private bool _enabled;
        private Vector3 _position;
        private Vector2 _origin;
        private Vector2 _scale;
        private float _rotation;
        private UIContainer _container;
        private UIAnchor _anchor;
        private int _drawOrder;

        public UIManager Manager { get; internal set; }
        public UIContainer Container { get => _container; set => SetContainer(value); }
        public UIAnchor Anchor { get => _anchor; set => SetAnchor(value); }
        public PivotPosition AnchorPivot => _anchor != null ? _anchor.Pivot : PivotPosition.None;
        public Vector3 AnchorOffset => _anchor != null ? _anchor.Offset : Vector3.Zero;

        public bool IsDisposed { get; private set; }
        public bool IsDrawable { get; protected set; }
        public bool IsEnabled { get => _enabled; set { SetEnabled(value); } }
        public bool IsActive => _enabled && (_container != null ? _container.IsEnabled : true);
        public bool IsDirty => DirtMarks != DirtMarkType.None;

        public int DrawOrder { get => _drawOrder; set => SetDrawOrder(value); }
        public DirtMarkType DirtMarks { get; private set; }
        public SamplingMode PreferredSampling { get; set; }

        public Vector3 Position { get => _position; set => SetPosition(value); }
        public Vector2 Origin { get => _origin; set => SetOrigin(value); }
        public Vector2 Scale { get => _scale; set => SetScale(value); }
        public float Rotation { get => _rotation; set => SetRotation(value); }
        public float X { get => _position.X; set => SetPositionF(value, _position.Y, _position.Z); }
        public float Y { get => _position.Y; set => SetPositionF(_position.X, value, _position.Z); }
        public float Z { get => _position.Z; set => SetPositionF(_position.X, _position.Y, value); }
        
        public float GlobalRotation => Container != null ? _rotation + Container.Rotation : _rotation;
        public Vector2 GlobalScale => Container != null ? _scale * Container.Scale : _scale;
        public Vector3 GlobalPosition
        {
            get
            {
                Vector3 pos = _position;
                if (_container != null)
                    pos += _container.Position;
                if (_anchor != null && _anchor.Pivot != PivotPosition.None)
                    pos += _anchor.Position;
                return pos;
            }
        }

        public UITransform(UIManager manager)
        {
            if (manager == null)
                throw new ArgumentNullException(nameof(manager));

            Manager.Add(this);
            PreferredSampling = manager.PreferredSampling;

            _scale = Vector2.One;
            IsDrawable = true;
            IsEnabled = true;
        }

        private void SetContainer(UIContainer value)
        {
            MarkDirty(ref _container, value, FULL_TRANSFORM_UPDATE);
        }
               
        private void SetDrawOrder(int value)
        {
            MarkDirty(ref _drawOrder, value, DirtMarkType.DrawOrder);
        }

        private void SetAnchor(UIAnchor value)
        {
            MarkDirty(ref _anchor, value, DirtMarkType.Position);
        }

        private void SetEnabled(bool value)
        {
            if (!_enabled && value && _updateViewportOnEnable)
            {
                ViewportChanged(Manager.Viewport);
                _updateViewportOnEnable = false;
            }
            MarkDirty(ref _enabled, value, DirtMarkType.Enabled);
        }

        public virtual void Update(GameTime time)
        {
        }

        public virtual void Draw(GameTime time, SpriteBatch batch)
        {
        }

        internal void OnViewportChange(Viewport viewport)
        {
            if (_enabled == false)
                _updateViewportOnEnable = true;
            else
                ViewportChanged(viewport);
        }

        public virtual void ViewportChanged(Viewport viewport)
        {
        }

        private void SetPosition(Vector3 value)
        {
            MarkDirty(ref _position, value, DirtMarkType.Position);
        }

        private void SetPositionF(float x, float y, float z)
        {
            SetPosition(new Vector3(x, y, z));
        }

        private void SetRotation(float value)
        {
            MarkDirty(ref _rotation, value, DirtMarkType.Rotation);
        }

        private void SetScale(Vector2 value)
        {
            MarkDirty(ref _scale, value, DirtMarkType.Scale);
        }

        private void SetOrigin(Vector2 value)
        {
            MarkDirty(ref _origin, value, DirtMarkType.Origin);
        }

        private bool AreEqual<T>(T x, T y)
        {
            if (x == null && y != null)
                return false;

            if (x is IEquatable<T> equatableX)
                return equatableX.Equals(y);

            return x.Equals(y);
        }
        
        protected bool MarkDirty<T>(ref T oldValue, T newValue, DirtMarkType marks)
        {
            if (oldValue == null || !AreEqual(oldValue, newValue))
            {
                oldValue = newValue;
                MarkDirty(marks);
                return true;
            }
            return false;
        }

        protected bool MarkDirty<T>(ref T oldValue, T newValue, DirtMarkType marks, IEqualityComparer<T> comparer)
        {
            if (oldValue == null || !comparer.Equals(oldValue, newValue))
            {
                oldValue = newValue;
                MarkDirty(marks);
                return true;
            }
            return false;
        }

        protected void MarkDirty(DirtMarkType marks, bool onlyMark)
        {
            DirtMarks |= marks;

            if (onlyMark == false)
            {
                MarkedDirty?.Invoke(this, marks);
                OnMarkedDirty(marks);
            }
        }

        protected void MarkDirty(DirtMarkType marks)
        {
            MarkDirty(marks, false);
        }

        internal void InvokeMarkedDirty(DirtMarkType marks)
        {
            MarkDirty(marks);
        }

        public bool HasAnyDirtMark(DirtMarkType marks)
        {
            return (DirtMarks & marks) != 0;
        }

        protected void MarkClean(DirtMarkType marks)
        {
            DirtMarks &= ~marks;
        }

        protected void MarkClean()
        {
            DirtMarks = DirtMarkType.None;
            MarkedClean?.Invoke(this);
        }

        protected virtual void OnMarkedDirty(DirtMarkType marks)
        {
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    if (!Manager.IsDisposed)
                        Manager.Remove(this);
                }

                IsDisposed = true;
            }
        }

        internal void FastDispose()
        {
            Dispose(false);
            GC.SuppressFinalize(this);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~UITransform()
        {
            Dispose(false);
        }
    }
}