using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.BitmapFonts;
using MonoGame.Extended.Collections;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using FontGlyph = MonoGame.Extended.BitmapFonts.BitmapFont.Glyph;
using FontDict = System.Collections.Generic.Dictionary<string, MonoGame.Extended.BitmapFonts.BitmapFont>;

namespace GeneralShare.UI
{
    public class TextRenderer
    {
        private BitmapFont _defaultFont;
        private FontDict _fonts;

        public IReadOnlyDictionary<string, BitmapFont> Fonts { get; }

        public BitmapFont DefaultFont
        {
            get => _defaultFont;
            set => _defaultFont = value ?? throw new ArgumentNullException(nameof(value));
        }

        public TextRenderer(BitmapFont defaultFont, FontDict fonts)
        {
            _defaultFont = defaultFont ?? throw new ArgumentNullException(nameof(defaultFont));
            _fonts = fonts == null ? new FontDict() : new FontDict(fonts);
            Fonts = new ReadOnlyDictionary<string, BitmapFont>(_fonts);
        }

        public TextRenderer(BitmapFont defaultFont) : this(defaultFont, null)
        {

        }

        public static ICharIterator GetColorFormat(
            BitmapFont font, ICharIterator chars, bool keepSequences, IReferenceList<Color?> output)
        {
            return TextFormat.ColorFormat(chars, font, keepSequences, output);
        }

        public static SizeF GetColoredSprites(
            BitmapFont font, ICharIterator chars, bool keepSequences,
            Vector2 position, Vector2 origin, Vector2 scale,
            IReferenceList<GlyphSprite> spriteOutput,
            IReferenceList<Color?> colorOutput)
        {
            using (var result = GetColorFormat(font, chars, keepSequences, colorOutput))
                return font.GetGlyphSprites(spriteOutput, result, position, Color.White, 0, origin, scale, 0, null);
        }
    }
}
