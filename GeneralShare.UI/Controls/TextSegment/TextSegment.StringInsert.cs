using MonoGame.Extended.BitmapFonts;
using System;

namespace GeneralShare.UI
{
    public partial class TextSegment
    {
        public void Insert(int index, ICharIterator chars, int offset, int count, bool useColorFormat)
        {
            if (index > CurrentText.Length)
                throw new ArgumentOutOfRangeException(nameof(index));

            var transform = new TransformCharIterator.Transform(offsetInSource: index, chars, offset, count);

            // leave source open as we dispose CurrentText in SetText()
            using (var iter = IteratorPool.Rent(
                CurrentText, leaveSourceOpen: true, transform, leaveTransformOpen: true))
            {
                SetText(_font, iter, useColorFormat);
            }

            BuildSprites(measure: false);
        }

        public void Insert(int index, ICharIterator chars, int offset, int count)
        {
            Insert(index, chars, offset, count, _usedColorFormat);
        }

        public void Insert(int index, ICharIterator chars, bool useColorFormat)
        {
            Insert(index, chars, 0, chars.Length, useColorFormat);
        }

        public void Insert(int index, ICharIterator chars)
        {
            Insert(index, chars, _usedColorFormat);
        }
    }
}