using MonoGame.Extended.BitmapFonts;
using System;
using System.Diagnostics;

namespace GeneralShare.UI
{
    public partial class TransformCharIterator : ICharIterator
    {
        private string _cachedString;
        internal bool _isInUse;

        private ICharIterator _source;
        private bool _leaveSourceOpen;

        private Transform _transform;
        private bool _leaveTransformOpen;

        public int Length { get; private set; }

        public TransformCharIterator(
            ICharIterator source, bool leaveSourceOpen,
            Transform transform, bool leaveTransformOpen)
        {
            Set(source, leaveSourceOpen, transform, leaveTransformOpen);
        }

        internal void Set(
            ICharIterator source, bool leaveSourceOpen,
            Transform transform, bool leaveTransformOpen)
        {
            if (!transform.IsValid || transform.OffsetInSource > source.Length)
                throw new ArgumentException(nameof(transform));

            if (transform.IsEraser && transform.OffsetInSource + transform.Count > source.Length)
                throw new ArgumentOutOfRangeException(nameof(transform));

            _source = source;
            _leaveSourceOpen = leaveSourceOpen;

            _leaveTransformOpen = leaveTransformOpen;
            _transform = transform;

            _cachedString = null;
            _isInUse = true;

            Length = source.Length + (transform.IsEraser ? -transform.Count : transform.Count);
        }

        public char GetCharacter16(int index)
        {
            AssertIsInUse();

            if (_transform.IsEraser)
            {
                // check if we are affected by the transform
                if (index >= _transform.OffsetInSource && index <= _transform.OffsetInSource + _transform.Count)
                    return _source.GetCharacter16(index + _transform.Count);
            }
            else
            {
                if (index >= _transform.OffsetInSource && index < _transform.OffsetInSource + _transform.Count)
                    return _transform.InsertionData.GetCharacter16(index - _transform.OffsetInSource + _transform.Offset);

                // we need to check if we are behind the transformed part
                if (index >= _transform.OffsetInSource + _transform.Count)
                    return _source.GetCharacter16(index - _transform.Count);
            }

            // otherwise just return
            return _source.GetCharacter16(index);
        }

        public int GetCharacter32(ref int index)
        {
            AssertIsInUse();

            char firstChar = GetCharacter16(index);
            return char.IsHighSurrogate(firstChar) && ++index < Length
                ? char.ConvertToUtf32(firstChar, GetCharacter16(index))
                : firstChar;
        }

        public string GetString()
        {
            AssertIsInUse();

            if (_cachedString == null)
            {
                var builder = StringBuilderPool.Rent(Length);
                builder.AppendIterator(this);

                _cachedString = builder.ToString();
                StringBuilderPool.Return(builder);
            }
            return _cachedString;
        }

        [DebuggerHidden]
        private void AssertIsInUse()
        {
            if (!_isInUse)
                throw new InvalidOperationException("This iterator is no longer valid.");
        }

        public override string ToString()
        {
            if (!_isInUse)
                return string.Empty;
            return GetString();
        }

        public void Dispose()
        {
            if (!_leaveSourceOpen)
                _source?.Dispose();
            _source = null;

            if (!_leaveTransformOpen)
                _transform.InsertionData?.Dispose();
            _transform = default;

            IteratorPool.Return(this);
        }
    }
}