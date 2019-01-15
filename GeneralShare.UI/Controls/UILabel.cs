using MonoGame.Extended.BitmapFonts;

namespace GeneralShare.UI
{
    public class UIText : UITextElement
    {
        protected override bool AllowTextColorFormatting => UseColorFormatting;
        public bool UseColorFormatting { get; set; }
        
        public ICharIterator Text { get => TextValue; set => TextValue = value; }

        public UIText(UIManager manager, BitmapFont font) : base(manager, font)
        {
            UseColorFormatting = true;
        }
    }
}