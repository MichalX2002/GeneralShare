using System;
using System.Text;

namespace GeneralShare.UI
{
    public static partial class TextFormat
    {
        public struct Input
        {
            private Func<int, char> _getChar;

            public char this[int index] => _getChar(index + Offset);
            public int Offset { get; }
            public int Length { get; }

            public Input(StringBuilder builder, int offset, int length)
            {
                if (builder == null)
                    throw new ArgumentNullException(nameof(builder));

                Offset = offset;
                Length = length;
                _getChar = (i) => builder[i];
            }

            public Input(string value, int offset, int length)
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));

                Offset = offset;
                Length = length;
                _getChar = (i) => value[i];
            }
        }
    }
}
