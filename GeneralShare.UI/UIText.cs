using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.BitmapFonts;
using System.Text;

namespace GeneralShare.UI
{
    public class UIText : UIElement
    {
        private BitmapFont _font;
        private RectangleF _boundaries;

        public ICharIterator Text { get; }
        public BitmapFont Font
        {
            get => _font;
            set
            {

            }
        }
        
        public override RectangleF Boundaries => _boundaries;

        public UIText(BitmapFont font) : base(null)
        {

        }

        public UIText(UIManager manager, BitmapFont font) : base(manager)
        {

        }



        public void SetText(string value, int offset, int length)
        {
            
        }

        public void SetText(string value)
        {
            SetText(value, 0, value.Length);
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
