using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Threading;

namespace GeneralShare.UI
{
    public class UITransform : IDisposable
    {
        private static int _lastTransformKey;

        public delegate void MarkedDirtyDelegate(DirtMarkType marks);
        public delegate void MarkedCleanDelegate();

        public event MarkedDirtyDelegate MarkedDirty;
        public event MarkedCleanDelegate MarkedClean;

        private bool _updateViewportOnEnable;
        private bool _dirty;
        private bool _enabled;
        private Vector3 _position;
        private Vector2 _scale;
        private float _rotation;
        private Vector2 _origin;
        private UIContainer _container;

        public readonly int TransformKey;
        public bool Disposed { get; private set; }
        public bool Enabled { get => _enabled; set { SetEnabled(value); } }
        public bool Active => _enabled && (_container == null ? true : _container.Enabled);
        public object SyncRoot { get; }
        public DirtMarkType DirtMarks { get; private set; }
        public UIManager Manager { get; }
        public UIContainer Container { get => _container; set => SetContainer(value); }

        public Vector2 Scale { get => GetScale(); set => SetScale(value); }
        public float Rotation { get => GetRotation(); set => SetRotation(value); }
        public Vector2 Origin { get => GetOrigin(); set => SetOrigin(value); }
        public Vector3 Position { get => GetPosition(); set => SetPosition(value); }
        public float X { get => Position.X; set => SetPositionF(value, _position.Y, _position.Z); }
        public float Y { get => Position.Y; set => SetPositionF(_position.X, value, _position.Z); }
        public float Z { get => Position.Z; set => SetPositionF(_position.X, _position.Y, value); }

        public bool Dirty
        {
            get => _dirty;
            protected set
            {
                _dirty = value;
                if (value == false)
                {
                    lock (SyncRoot)
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
            }

            Enabled = true;
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

        public void SetContainer(UIContainer container)
        {
            MarkDirty(ref container, container, DirtMarkType.Transform |
                DirtMarkType.Position | DirtMarkType.Origin |
                DirtMarkType.Scale | DirtMarkType.Rotation);
        }

        private Vector2 GetScale()
        {
            return Container == null ? _scale : _scale + Container.Scale;
        }

        private float GetRotation()
        {
            return Container == null ? _rotation : _rotation + Container.Rotation;
        }

        private Vector2 GetOrigin()
        {
            return Container == null ? _origin : _origin + Container.Origin;
        }
        
        public Vector3 GetPosition()
        {
            return Container == null ? _position : _position + Container.Position;
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
            _enabled = value;
        }

        public virtual void Update(GameTime time)
        {
        }

        internal void OnViewportChange(in Viewport viewport)
        {
            if (_enabled == false)
                _updateViewportOnEnable = true;
            else
                ViewportChanged(viewport);
        }

        public virtual void ViewportChanged(in Viewport viewport)
        {
        }

        public bool HasDirtMarks(params DirtMarkType[] types)
        {
            var src = DirtMarks;
            for (int i = 0; i < types.Length; i++)
            {
                if ((src & types[i]) != 0)
                    return true;
            }
            return false;
        }

        public bool HasDirtMarks(DirtMarkType types)
        {
            return (DirtMarks & types) != 0;
        }

        private void SetPosition(in Vector3 value)
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

        private void SetScale(in Vector2 value)
        {
            MarkDirtyE(ref _scale, value, DirtMarkType.Scale | DirtMarkType.Transform);
        }

        private void SetOrigin(in Vector2 value)
        {
            MarkDirtyE(ref _origin, value, DirtMarkType.Origin | DirtMarkType.Transform);
        }

        protected void ClearDirtMarks(DirtMarkType types)
        {
            DirtMarks &= ~types;
        }

        protected void ClearDirtMarks()
        {
            DirtMarks = 0;
        }

        protected bool MarkDirtyE<T>(ref T oldValue, T newValue, DirtMarkType types)
            where T : IEquatable<T>
        {
            if (oldValue == null || oldValue.Equals(newValue) == false)
            {
                oldValue = newValue;
                MarkDirty(types);
                return true;
            }
            return false;
        }

        protected bool MarkDirty<T>(ref T oldValue, T newValue, DirtMarkType types)
        {
            if (oldValue == null || oldValue.Equals(newValue) == false)
            {
                oldValue = newValue;
                MarkDirty(types);
                return true;
            }
            return false;
        }

        protected bool MarkDirty<T>(
            ref T oldValue, T newValue, DirtMarkType types, IEqualityComparer<T> comparer)
        {
            if (oldValue == null || comparer.Equals(oldValue, newValue) == false)
            {
                oldValue = newValue;
                MarkDirty(types);
                return true;
            }
            return false;
        }

        protected void MarkDirty(DirtMarkType types, bool onlyMarkFlag)
        {
            if (onlyMarkFlag == false)
                _dirty = true;
            DirtMarks |= types;
            InvokeMarkedDirty(types);
        }

        protected void MarkDirty(DirtMarkType types)
        {
            MarkDirty(types, false);
        }

        protected void InvokeMarkedDirty(DirtMarkType types)
        {
            lock (SyncRoot)
            {
                MarkedDirty?.Invoke(types);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!Disposed)
            {
                if (disposing)
                {
                    if (Manager != null)
                        Manager.Remove(this);
                }

                Disposed = true;
            }
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