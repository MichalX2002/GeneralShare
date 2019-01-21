using Microsoft.Xna.Framework;
using MonoGame.Extended.BitmapFonts;

namespace GeneralShare.UI
{
    public abstract partial class UITextElement : UIElement
    {
        #region Property Setters
        private void SetFont(BitmapFont value)
        {
            MarkDirty(ref _font, value, DirtMarkType.Font);
        }

        protected void SetTextValue(ICharIterator iterator)
        {
            using (iterator)
                _segment.SetText(_font, iterator, AllowTextColorFormatting);
            MarkDirty(DirtMarkType.Value);
        }

        private void SetColor(Color value)
        {
            if (MarkDirty(Color, value, DirtMarkType.Color))
                _segment.Color = value;
        }

        private void SetAlignment(TextAlignment value)
        {
            MarkDirty(ref _alignment, value, DirtMarkType.TextAlignment);
        }

        private void SetIsShadowed(bool value)
        {
            MarkDirty(ref _shadowed, value, DirtMarkType.Shadowed);
        }

        private void SetClipRect(RectangleF? value)
        {
            if (MarkDirty(_segment.ClipRect, value, DirtMarkType.ClipRect))
                _segment.ClipRect = value;
        }
        #endregion

        #region Property Getters
        private RectangleF GetBoundaries()
        {
            AssertPure();
            return _boundaries;
        }

        protected virtual RectangleF GetStringRect()
        {
            AssertPure();
            return _stringRect;
        }
        #endregion
    }
}
