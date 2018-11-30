using MonoGame.Extended.BitmapFonts;
using MonoGame.Extended.Collections;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using FontGlyph = MonoGame.Extended.BitmapFonts.BitmapFont.Glyph;

namespace GeneralShare.UI
{
    public class TextRenderer
    {
        private BitmapFont _defaultFont;
        private Dictionary<string, BitmapFont> _fonts;

        public IReadOnlyDictionary<string, BitmapFont> Fonts { get; }

        public BitmapFont DefaultFont
        {
            get => _defaultFont;
            set => _defaultFont = value ?? throw new ArgumentNullException(nameof(value));
        }

        public TextRenderer(BitmapFont defaultFont, Dictionary<string, BitmapFont> fonts)
        {
            _defaultFont = defaultFont ?? throw new ArgumentNullException(nameof(defaultFont));
            _fonts = new Dictionary<string, BitmapFont>(fonts ?? throw new ArgumentNullException(nameof(fonts)));
            Fonts = new ReadOnlyDictionary<string, BitmapFont>(_fonts);
        }

        public void GetGlyphs(ICharIterator chars, IReferenceList<FontGlyph> output)
        {
            
        }
    }
}
