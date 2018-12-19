using GeneralShare.Collections;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.BitmapFonts;
using MonoGame.Extended.Collections;

namespace GeneralShare.UI
{
    public static class TextSegmentPool
    {
        private static Bag<TextSegment> _bag;

    }

    public class TextSegment
    {
        private ListArray<Color> _formatColorList;
        internal ListArray<GlyphSprite> _spriteList;
        private SizeF _size;

        public int Count => _spriteList.Count;

        public TextSegment()
        {
            _formatColorList = new ListArray<Color>();
            _spriteList = new ListArray<GlyphSprite>();
        }

        public void Set(BitmapFont font, ICharIterator iterator, Color color)
        {
            _formatColorList.Clear();
            _spriteList.Clear();

            using (var formatResult = TextRenderer.GetColorFormat(font, iterator, color, false, _formatColorList))
            {
                _size = font.GetGlyphSprites(
                    _spriteList, formatResult, Vector2.Zero, color, 0, Vector2.Zero, Vector2.One, 0, null);
            }
        }
    }
}
