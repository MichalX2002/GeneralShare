using MonoGame.Extended.BitmapFonts;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace GeneralShare.UI
{
    public class TextRenderer
    {
        private Dictionary<string, BitmapFont> _fonts;

        public IReadOnlyDictionary<string, BitmapFont> Fonts { get; }

        public TextRenderer(Dictionary<string, BitmapFont> fonts)
        {
            _fonts = new Dictionary<string, BitmapFont>(fonts);
            Fonts = new ReadOnlyDictionary<string, BitmapFont>(_fonts);
        }


    }
}
