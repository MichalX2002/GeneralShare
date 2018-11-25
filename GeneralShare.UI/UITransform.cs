using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Threading;

namespace GeneralShare.UI
{
    public class UITransform : IDisposable
    {
        internal const DirtMarkType FULL_TRANSFORM_UPDATE =
            DirtMarkType.Transform | DirtMarkType.Position |
            DirtMarkType.Origin | DirtMarkType.Scale | DirtMarkType.Rotation;

        private static int _lastTransformKey;

        public delegate void MarkedDirtyDelegate(DirtMarkType marks);
        public delegate void MarkedCleanDelegate();

        public event MarkedDirtyDelegate MarkedDirty;
        public event MarkedCleanDelegate MarkedClean;

        private bool _updateViewportOnEnable;
        private bool _dirty;
        private bool _enabled;
        private Vector3 _position;
        private Vector2 _origin;
        private Vector2 _scale;
        private float _rotation;
        private UIContainer _container;
        private UIAnchor _anchor;
        private int _drawOrder;

        public readonly int TransformKey;
        public object SyncRoot { get; }
        public bool IsDisposed { get; private set; }
        public bool IsDrawable { get; protected set; }
        public bool IsEnabled { get => _enabled; set { SetEnabled(value); } }
        public bool IsActive => _enabled && (_container == null ? true : _container.IsEnabled);
        public int DrawOrder { get => _drawOrder; set => SetDrawOrder(value); }
        public DirtMarkType DirtMarks { get; private set; }
        public SamplingMode PreferredSampling { get; set; }

        public UIManager Manager { get; }
        public UIContainer Container { get => _container; set => SetContainer(value); }
        public UIAnchor Anchor { get => _anchor; set => SetAnchor(value); }
        public PivotPosition AnchorPivot => _anchor == null ? _anchor.Pivot : PivotPosition.None;
        public Vector3 AnchorOffset => _anchor == null ? _anchor.Offset : Vector3.Zero;
        
        public Vector3 GlobalPosition => GetGlobalPosition();
        public Vector2 GlobalScale => GetGlobalScale();   
        public float GlobalRotation => GetGlobalRotation();

        public Vector3 Position { get => _position; set => SetPosition(value); }
        public Vector2 Origin { get => _origin; set => SetOrigin(value); }
        public Vector2 Scale { get => _scale; set => SetScale(value); }
        public float Rotation { get => _rotation; set => SetRotation(value); }
        public float X { get => _position.X; set => SetPositionF(value, _position.Y, _position.Z); }
        public float Y { get => _position.Y; set => SetPositionF(_position.X, value, _position.Z); }
        public float Z { get => _position.Z; set => SetPositionF(_position.X, _position.Y, value); }

        public bool Dirty
        {
            get => _dirty;
            protected set
            {
                lock (SyncRoot)
                {
                    _dirty = value;
                    if (value == false)
                    {
                        MarkedClean?.Invoke();
                    }
                }
            }
        }

        private UITransform(UIManager manager, Vector2 scale)
        {
            TransformKey = Interlocked.Increment(ref _lastTransformKey);
            SyncRoot = new object();
            _scale = scale;

            if (manager != null)
            {
                Manager = manager;
                Manager.Add(this);
                PreferredSampling = manager.PreferredSamplingMode;
            }
            else
            {
                PreferredSampling = SamplingMode.LinearClamp;
            }

            IsDrawable = true;
            IsEnabled = true;
        }

        public UITransform(UIManager manager) : this(manager, Vector2.One)
        {
        }

        public UITransform() : this(null)
        {
        }

        public UITransform(Vector3 position, Vector2 scale, float rotation, Vector2 origin) : this(null, scale)
        {
            _position = position;
            _rotation = rotation;
            _origin = origin;
        }

        internal void SetContainer(UIContainer value)
        {
            MarkDirty(ref _container, value, FULL_TRANSFORM_UPDATE);
        }
               
        private void SetDrawOrder(int value)
        {
            MarkDirtyE(ref _drawOrder, value, DirtMarkType.DrawOrder);
        }

        private void SetAnchor(UIAnchor value)
        {
            MarkDirty(ref _anchor, value, DirtMarkType.Position);
        }

        private Vector2 GetGlobalScale()
        {
            if (Container == null)
                return _scale;
            return _scale * Container.Scale;
        }

        private float GetGlobalRotation()
        {
            if (Container == null)
                return _rotation;
            return _rotation + Container.Rotation;
        }
        
        private Vector3 GetGlobalPosition()
        {
            Vector3 pos = _position;
            if(_container != null)
                pos += _container.Position;
            if (_anchor != null && _anchor.Pivot != PivotPosition.None)
                pos += _anchor.Position;
            return pos;
        }

        private void SetEnabled(bool value)
        {
            if (Manager != null)
            {
                if (!_enabled && value && _updateViewportOnEnable)
                {
                    ViewportChanged(Manager._lastViewport);
                    _updateViewportOnEnable = false;
                }
            }
            MarkDirtyE(ref _enabled, value, DirtMarkType.Enabled);
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
            MarkDirtyE(ref _position, value, DirtMarkType.Position | DirtMarkType.Transform);
        }

        private void SetPositionF(float x, float y, float z)
        {
            SetPosition(new Vector3(x, y, z));
        }

        private void SetRotation(float value)
        {
            MarkDirtyE(ref _rotation, value, DirtMarkType.Rotation | DirtMarkType.Transform);
        }

        private void SetScale(Vector2 value)
        {
            MarkDirtyE(ref _scale, value, DirtMarkType.Scale | DirtMarkType.Transform);
        }

        private void SetOrigin(Vector2 value)
        {
            MarkDirtyE(ref _origin, value, DirtMarkType.Origin | DirtMarkType.Transform);
        }

        public bool HasAnyDirtMark(DirtMarkType marks)
        {
            return (DirtMarks & marks) != 0;
        }

        protected void ClearDirtMarks(DirtMarkType marks)
        {
            DirtMarks &= ~marks;
        }

        protected void ClearDirtMarks()
        {
            DirtMarks = 0;
        }

        protected bool MarkDirtyE<T>(ref T oldValue, T newValue, DirtMarkType marks)
            where T : IEquatable<T>
        {
            if (oldValue == null || oldValue.Equals(newValue) == false)
            {
                oldValue = newValue;
                MarkDirty(marks);
                return true;
            }
            return false;
        }

        protected bool MarkDirty<T>(ref T oldValue, T newValue, DirtMarkType marks)
        {
            if (oldValue == null || oldValue.Equals(newValue) == false)
            {
                oldValue = newValue;
                MarkDirty(marks);
                return true;
            }
            return false;
        }

        protected bool MarkDirty<T>(
            ref T oldValue, T newValue, DirtMarkType marks, IEqualityComparer<T> comparer)
        {
            if (oldValue == null || comparer.Equals(oldValue, newValue) == false)
            {
                oldValue = newValue;
                MarkDirty(marks);
                return true;
            }
            return false;
        }

        protected void MarkDirty(DirtMarkType marks, bool onlyMark)
        {
            _dirty = true;
            DirtMarks |= marks;

            if (onlyMark == false)
                InvokeMarkedDirty(marks);
        }

        protected void MarkDirty(DirtMarkType marks)
        {
            MarkDirty(marks, false);
        }

        protected void InvokeMarkedDirty(DirtMarkType marks)
        {
            lock (SyncRoot)
            {
                MarkedDirty?.Invoke(marks);
            }
        }

        internal void InvokeMarkedDirtyInternal(DirtMarkType marks)
        {
            MarkDirty(marks);
        }

        public override bool Equals(object obj)
        {
            if (obj is UITransform transform)
                return TransformKey == transform.TransformKey;
            return false;
        }

        public override int GetHashCode()
        {
            return TransformKey;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    if (Manager != null)
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