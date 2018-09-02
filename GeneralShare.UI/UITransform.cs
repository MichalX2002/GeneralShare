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
        private Vector2 _scale;
        private float _rotation;
        private Vector2 _origin;
        private UIContainer _container;

        public readonly int TransformKey;
        public bool IsDisposed { get; private set; }
        public bool IsEnabled { get => _enabled; set { SetEnabled(value); } }
        public bool IsActive => _enabled && (_container == null ? true : _container.IsEnabled);
        public object SyncRoot { get; }
        public DirtMarkType DirtMarks { get; private set; }
        public UIManager Manager { get; }
        public UIContainer Container => _container;

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

        internal void SetContainer(UIContainer container)
        {
            MarkDirty(ref _container, container, FULL_TRANSFORM_UPDATE);
        }

        private Vector2 GetScale()
        {
            return Container == null ? _scale : _scale * Container.Scale;
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

        internal void OnViewportChange(Viewport viewport)
        {
            if (_enabled == false)
                _updateViewportOnEnable = true;
            else
                ViewportChanged(viewport);
        }

        public virtual void ViewportChanged(in Viewport viewport)
        {
        }
        public bool HasDirtMarks(params DirtMarkType[] marks)
        {
            var src = DirtMarks;
            for (int i = 0; i < marks.Length; i++)
            {
                if ((src & marks[i]) != 0)
                    return true;
            }
            return false;
        }

        public bool HasDirtMarks(DirtMarkType marks)
        {
            return (DirtMarks & marks) != 0;
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

        protected void MarkDirty(DirtMarkType marks, bool onlyMarkFlags)
        {
            if (onlyMarkFlags == false)
                _dirty = true;
            DirtMarks |= marks;
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