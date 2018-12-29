using GeneralShare.Collections;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.BitmapFonts;
using MonoGame.Extended.Collections;
using System.Collections.Generic;
using FontGlyph = MonoGame.Extended.BitmapFonts.BitmapFont.Glyph;

namespace GeneralShare.UI
{
    public static class TextSegmentPool
    {
        private static Bag<TextSegment> _bag;
    }

    class SpriteCharIterator : ICharIterator
    {
        private IList<GlyphSprite> _sprites;
        private int _offset;

        public int Length { get; private set; }

        public SpriteCharIterator(IList<GlyphSprite> sprites, int offset, int count)
        {
            Set(sprites, offset, count);
        }

        public void Set(IList<GlyphSprite> sprites, int offset, int count)
        {
            _sprites = sprites;
            _offset = offset;
            Length = count;
        }

        public char GetCharacter16(int index)
        {
            return (char)_sprites[index + _offset].Char;
        }

        public int GetCharacter32(ref int index)
        {
            return _sprites[index + _offset].Char;
        }

        public void Dispose()
        {
        }
    }

    public class TextSegment
    {
        //private SpriteCharIterator _iter;

        private ListArray<FontGlyph> _glyphList;
        private ListArray<Color?> _formatColorList;
        internal ListArray<GlyphSprite> _spriteList;
        private BitmapFont _font;
        private Color _color;
        private Vector2 _scale;

        public int Count => _spriteList.Count;
        public SizeF Measure { get; private set; }
        public Color Color
        {
            get => _color;
            set
            {
                _color = value;
            }
        }

        public Vector2 Scale
        {
            get => _scale;
            set
            {
                _scale = value;
            }
        }

        public TextSegment()
        {
            _glyphList = new ListArray<FontGlyph>();
            _formatColorList = new ListArray<Color?>();
            _spriteList = new ListArray<GlyphSprite>();
            //_iter = new SpriteCharIterator(null, 0, 0);

            _color = Color.White;
            _scale = Vector2.One;
        }

        public void Initialize(BitmapFont font, ICharIterator iterator)
        {
            _font = font;
            _formatColorList.Clear();
            _glyphList.Clear();

            using (var formatResult = TextRenderer.GetColorFormat(_font, iterator, false, _formatColorList))
            {
                Measure = _font.GetGlyphs(formatResult, _glyphList) * _scale;
                BuildSprites(false);
            }
        }

        private void BuildSprites(bool measure)
        {
            if (_font == null || _glyphList.Count == 0)
                return;

            _spriteList.Clear();
            BitmapFontExtensions.GetGlyphSprites(
                _glyphList, _spriteList, Vector2.Zero, _color, 0, Vector2.Zero, _scale, 0, null);

            if (measure)
                Measure = _font.MeasureString(_glyphList) * _scale;

            // apply the colors from the formatting
            for (int i = 0; i < _formatColorList.Count; i++)
            {
                Color? nullable = _formatColorList[i];
                if (nullable.HasValue)
                    _spriteList.GetReferenceAt(i).Color = nullable.Value;
            }
        }

        public void Rebuild()
        {
            BuildSprites(true);
        }
    }
}
