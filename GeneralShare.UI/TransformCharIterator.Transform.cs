using MonoGame.Extended.BitmapFonts;
using System;

namespace GeneralShare.UI
{
    public partial class TransformCharIterator
    {
        public readonly struct Transform
        {
            public bool IsEraser { get; }
            public int OffsetInSource { get; }
            public ICharIterator InsertionData { get; }
            public int Offset { get; }
            public int Count { get; }

            public bool IsValid =>
                IsEraser ? InsertionData == null : InsertionData != null && // check if this transform needs data
                OffsetInSource >= 0 &&
                OffsetInSource < Count &&
                InsertionData == null ? true : InsertionData.Length <= Count && // check if we don't crave more than the data offers 
                Offset >= 0 &&
                InsertionData == null ? true : Offset + Count <= InsertionData.Length && // check if we are inside the boundaries of the data
                Count > 0;

            private Transform(bool isEraser, int offsetInSource, ICharIterator insertionData, int offset, int count)
            {
                IsEraser = isEraser;
                OffsetInSource = offsetInSource;
                InsertionData = insertionData;
                Offset = offset;
                Count = count;
            }

            public Transform(
                int offsetInSource, ICharIterator insertionData, int offset, int count) :
                this(false, offsetInSource, insertionData, offset, count)
            {
                if (insertionData == null)
                    throw new ArgumentNullException(nameof(insertionData));
            }

            public Transform(int offsetInSource, int count) :
                this(true, offsetInSource, null, 0, count)
            {
            }
        }
    }
}
