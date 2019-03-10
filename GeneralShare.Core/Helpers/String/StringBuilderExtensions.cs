
using MonoGame.Extended.BitmapFonts;
using System;
using System.Text;

namespace GeneralShare
{
    public static class StringBuilderExtensions
    {
        public static void AppendIterator(this StringBuilder builder, ICharIterator iterator, int offset, int count)
        {
            if (count > iterator.Length)
                throw new ArgumentOutOfRangeException(nameof(count));

            if (offset + count > iterator.Length)
                throw new ArgumentOutOfRangeException(nameof(offset));

            builder.EnsureCapacity(count);
            for (int i = 0; i < count; i++)
                builder.Append(iterator.GetCharacter16(i + offset));
        }

        public static void AppendIterator(this StringBuilder builder, ICharIterator iterator)
        {
            AppendIterator(builder, iterator, 0, iterator.Length);
        }
    }
}
