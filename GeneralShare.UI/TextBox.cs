using MonoGame.Extended.BitmapFonts;

namespace GeneralShare.UI
{
    public class TextBox : TextBoxBase
    {
        public bool UseTextExpressions { get => _valueExpressions; set => SetValueExp(value); }
        public bool KeepTextExpressions { get => _keepExpressions; set => SetKeepExp(value); }
        public string Text { get => _value; set => SetValue(value); }

        public TextBox(UIManager manager, BitmapFont font) : base(manager, font)
        {
            UseTextExpressions = true;
            KeepTextExpressions = false;
        }

        public TextBox(BitmapFont font) : this(null, font)
        {
        }
    }
}