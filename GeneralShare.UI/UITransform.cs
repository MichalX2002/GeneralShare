using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace GeneralShare.UI
{
    public class UITransform
    {
        public delegate void MarkedDirtyDelegate(DirtMarkType type);
        public delegate void MarkedCleanDelegate();

        public event MarkedDirtyDelegate MarkedDirty;
        public event MarkedCleanDelegate MarkedClean;

        protected readonly object _syncRoot;
        private bool _dirty;
        protected Vector3 _position;
        protected Vector2 _scale;
        protected float _rotation;
        protected Vector2 _origin;

        public object SyncRoot => _syncRoot;
        public DirtMarkType DirtMarks { get; private set; }
        public Vector3 Position { get => _position; set => SetPosition(value); }
        public float X { get => _position.X; set => SetPositionF(value, _position.Y, _position.Z); }
        public float Y { get => _position.Y; set => SetPositionF(_position.X, value, _position.Z); }
        public float Z { get => _position.Z; set => SetPositionF(_position.X, _position.Y, value); }

        public Vector2 Scale { get => _scale; set => SetScale(value); }
        public float Rotation { get => _rotation; set => SetRotation(value); }
        public Vector2 Origin { get => _origin; set => SetOrigin(value); }

        public bool Dirty
        {
            get => _dirty;
            protected set
            {
                _dirty = value;
                if (value == false)
                {
                    lock (_syncRoot)
                    {
                        MarkedClean?.Invoke();
                    }
                }
            }
        }

        public UITransform(Vector3 position, Vector2 scale, float rotation, Vector2 origin)
        {
            _syncRoot = new object();
            _position = position;
            _scale = scale;
            _rotation = rotation;
            _origin = origin;
        }

        public UITransform() : this(Vector3.Zero, Vector2.One, 0, Vector2.Zero)
        {
        }

        public bool HasDirtMarks(params DirtMarkType[] flags)
        {
            var src = DirtMarks;
            for (int i = 0; i < flags.Length; i++)
            {
                if ((src & flags[i]) != 0)
                    return true;
            }
            return false;
        }

        public bool HasDirtMarks(DirtMarkType flags)
        {
            return (DirtMarks & flags) != 0;
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

        protected bool MarkDirtyE<T>(ref T oldValue, in T newValue, DirtMarkType type) where T : IEquatable<T>
        {
            if (oldValue == null || oldValue.Equals(newValue) == false)
            {
                oldValue = newValue;
                MarkDirty(type);
                return true;
            }
            return false;
        }

        protected bool MarkDirty<T>(ref T oldValue, in T newValue, DirtMarkType type)
        {
            if (oldValue == null || oldValue.Equals(newValue) == false)
            {
                oldValue = newValue;
                MarkDirty(type);
                return true;
            }
            return false;
        }

        protected bool MarkDirty<T>(ref T oldValue, in T newValue,
            DirtMarkType type, in IEqualityComparer<T> comparer)
        {
            if (oldValue == null || comparer.Equals(oldValue, newValue) == false)
            {
                oldValue = newValue;
                MarkDirty(type);
                return true;
            }
            return false;
        }

        protected void MarkDirty(DirtMarkType type, bool onlyMarkFlag)
        {
            if (onlyMarkFlag == false)
                _dirty = true;
            DirtMarks |= type;

            lock (_syncRoot)
            {
                MarkedDirty?.Invoke(type);
            }
        }

        protected void MarkDirty(DirtMarkType type)
        {
            MarkDirty(type, false);
        }
    }
}