using MonoGame.Extended.BitmapFonts;

namespace GeneralShare.UI
{
    public class TextBox : TextBoxBase
    {
        public bool UseTextExpressions { get => _valueExpressions; set => SetValueExp(value); }
        public bool KeepTextExpressions { get => _keepExpressions; set => SetKeepExp(value); }
        public string Text { get => _value; set => SetValue(value); }

        public TextBox(UIContainer container, BitmapFont font) : base(container, font)
        {
            UseTextExpressions = true;
            KeepTextExpressions = false;
        }
    }
}