using MonoGame.Extended.BitmapFonts;
using System;
using System.Collections.Generic;
using System.Text;

namespace GeneralShare.UI
{
    public partial class TextSegment
    {
        public void Remove(int index, int count)
        {
            if (index > CurrentText.Length)
                throw new ArgumentOutOfRangeException(nameof(index));

            var transform = new TransformCharIterator.Transform(offsetInSource: index, count);

            // leave source open as we dispose CurrentText in SetText()
            using (var iter = IteratorPool.Rent(
                CurrentText, leaveSourceOpen: true, transform, leaveTransformOpen: false))
            {
                SetText(_font, iter, _usedColorFormat);
            }
        }

        public void Remove(IEnumerable<char> chars)
        {
            StringBuilder tmpBuilder = null;
            int removals = 0;

            foreach (int c in chars)
                Remove(ref tmpBuilder, ref removals, c);
            ProcessRemoval(tmpBuilder, removals);
        }

        public void Remove(char value)
        {
            StringBuilder tmpBuilder = null;
            int removals = 0;

            Remove(ref tmpBuilder, ref removals, value);
            ProcessRemoval(tmpBuilder, removals);
        }

        private void Remove(ref StringBuilder tmpBuilder, ref int removals, int value)
        {
            for (int i = 0; i < CurrentText.Length; i++)
            {
                char current = CurrentText.GetCharacter16(i);
                if (current == value)
                {
                    if (tmpBuilder == null)
                    {
                        tmpBuilder = StringBuilderPool.Rent(CurrentText.Length);

                        // append the data that we were holding onto,
                        // excluding the char we are on currently
                        for (int j = 0; j < i; j++)
                            tmpBuilder.Append(CurrentText.GetCharacter16(j));
                    }

                    removals++;
                }
                else if (tmpBuilder != null) // we don't append if there are no changes yet
                    tmpBuilder.Append(current);
            }
        }

        private void ProcessRemoval(StringBuilder builder, int removals)
        {
            if (removals > 0)
            {
                using (var iter = builder.ToIterator())
                    SetText(_font, iter, _usedColorFormat);

                StringBuilderPool.Return(builder);
            }
        }
    }
}
