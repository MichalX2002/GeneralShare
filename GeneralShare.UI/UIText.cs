using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.BitmapFonts;
using System.Text;

namespace GeneralShare.UI
{
    public class UIText : UIElement
    {
        //public ICharIterator Text { get; }
        public string Text;
        public Color BaseColor;
        public Vector2 Scale = new Vector2(2f);
        public bool UseShadow = true;
        public Color ShadowColor = new Color(245, 245, 245, 150);
        public TextAlignment Alignment;
        public SizeF Measure;
        public RectangleF ShadowOffset = new RectangleF(-3, -4, 7, 7);

        public override RectangleF Boundaries => Rectangle.Empty;

        public UIText(BitmapFont font) : base(null)
        {

        }

        public UIText(UIManager manager, BitmapFont font) : base(manager)
        {

        }

        public void SetText(string value, int offset, int length)
        {
            
        }

        public void SetText(StringBuilder value, int offset, int length)
        {

        }

        public void SetText(StringBuilder value)
        {
            SetText(value, 0, value.Length);
        }
    }
}
