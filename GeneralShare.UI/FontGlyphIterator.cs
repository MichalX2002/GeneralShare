using MonoGame.Extended.BitmapFonts;
using System;
using System.Collections.Generic;

namespace GeneralShare.UI
{
    public class FontGlyphIterator : ICharIterator
    {
        private string _cachedString;

        private IList<BitmapFont.Glyph> _sprites;
        private int _offset;

        public int Length { get; private set; }

        public FontGlyphIterator(IList<BitmapFont.Glyph> sprites, int offset, int count)
        {
            Set(sprites, offset, count);
        }

        public void Set(IList<BitmapFont.Glyph> sprites, int offset, int count)
        {
            _sprites = sprites;
            _offset = offset;
            Length = count;

            _cachedString = null;
        }

        public char GetCharacter16(int index)
        {
            return (char)_sprites[index + _offset].Character;
        }

        public int GetCharacter32(ref int index)
        {
            int c = _sprites[index + _offset].Character;
            if (char.IsHighSurrogate((char)c))
                index++;
            return c;
        }

        public string GetString()
        {
            if (_sprites == null)
                throw new System.Exception();

            if(_cachedString == null)
            {
                var builder = StringBuilderPool.Rent(Length);
                Span<char> charBuffer = stackalloc char[2];

                for (int i = 0; i < Length; i++)
                {
                    int utf32 = _sprites[i].Character;
                    int count = TextFormat.ConvertFromUtf32(utf32, charBuffer);

                    for (int j = 0; j < count; j++)
                        builder.Append(charBuffer[j]);
                }

                _cachedString = builder.ToString();
                StringBuilderPool.Return(builder);
            }

            return _cachedString;
        }

        public override string ToString()
        {
            if (_sprites == null)
                return string.Empty;
            return GetString();
        }

        public void Dispose()
        {
            Set(null, 0, 0);
        }
    }
}
