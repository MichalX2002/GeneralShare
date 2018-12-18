using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.BitmapFonts;

namespace GeneralShare.UI
{
    public class UIInput : UIElement
    {
        public bool TriggerMouseEvents;
        public string Value;
        public Color ShadowColor;
        public bool UseShadow;

        public BitmapFont Font;

        public string Prefix;
        public bool AllowNewLine;
        public TextAlignment Alignment;
        public SizeF Measure;

        public override RectangleF Boundaries => Rectangle.Empty;

        public UIInput(UIManager manager, BitmapFont font) : base(manager)
        {

        }

        public void AddCharToBlacklist(char value)
        {

        }
    }
}