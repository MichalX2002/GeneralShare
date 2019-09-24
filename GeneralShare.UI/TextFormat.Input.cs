using MonoGame.Extended.BitmapFonts;
using System;
using System.Text;

namespace GeneralShare.UI
{
    public static partial class TextFormat
    {
        public struct TextInput : BitmapFont.ITextIterator
        {
            private Func<int, char> _getChar;
            
            public int Offset { get; }
            public int Count { get; }
            public int TotalCount { get; }

            public TextInput(StringBuilder builder, int offset, int count)
            {
                if (builder == null)
                    throw new ArgumentNullException(nameof(builder));

                Offset = offset;
                Count = count;
                TotalCount = builder.Length;
                _getChar = (i) => builder[i];
            }

            public TextInput(string value, int offset, int count)
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));

                Offset = offset;
                Count = count;
                TotalCount = value.Length;
                _getChar = (i) => value[i];
            }

            public int GetCharacter(ref int index)
            {
                return char.IsHighSurrogate(_getChar(index)) && ++index < TotalCount
                    ? char.ConvertToUtf32(_getChar(index - 1), _getChar(index))
                    : _getChar(index);
            }
        }
    }
}
