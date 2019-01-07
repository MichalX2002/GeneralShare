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

        private void SetClipRect(RectangleF? value)
        {
            if (MarkDirty(_segment.ClipRect, value, DirtMarkType.ClipRect))
                _segment.ClipRect = value;
        }
        #endregion

        #region Property Getters
        private RectangleF GetBounds()
        {
            AssertPure();
            return _boundaries;
        }

        protected virtual SizeF GetMeasure()
        {
            AssertPure();
            return _segment.Measure;
        }

        protected virtual int GetSpriteCount()
        {
            int count = _segment.SpriteCount;
            if (IsShadowed && _segment.SpriteCount > 0)
                count++;
            return count;
        }
        #endregion
    }
}
