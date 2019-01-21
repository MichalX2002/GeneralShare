using Microsoft.Xna.Framework;
using MonoGame.Extended.BitmapFonts;

namespace GeneralShare.UI
{
    public partial class UITextField : UITextElement
    {
        #region Property Setters
        private void SetPlaceholder(ICharIterator value)
        {
            using (value)
                _placeholderSegment.SetText(Font, value, UsePlaceholderColorFormatting);

            MarkDirty(DirtMarkType.Placeholder);
        }

        private void SetPlaceholderColor(Color value)
        {
            MarkDirty(ref _placeholderColor, value, DirtMarkType.PlaceholderColor);
        }

        private void SetCharLimit(int value)
        {
            if (_charLimit != value)
            {
                _charLimit = value;
                if (Value.Length > _charLimit)
                    Remove(_charLimit, Value.Length - _charLimit);
            }
        }

        private void SetIsMultiLined(bool value)
        {
            if (_isMultiLined != value)
            {
                _isMultiLined = value;
                UpdateGlyphs();
            }
        }

        private void SetIsObscured(bool value)
        {
            if (_isObscured != value)
            {
                _isObscured = value;
                UpdateGlyphs();
            }
        }

        private void SetObscureChar(char value)
        {
            if (_obscureChar != value)
            {
                _obscureChar = value;
                if (_isObscured)
                    UpdateGlyphs();
            }
        }
        #endregion

        #region Property Getters
        protected override SizeF GetMeasure()
        {
            SizeF measure = base.GetMeasure();
            if (measure == SizeF.Empty)
                measure = _placeholderSegment.Measure;
            else
                measure.Width += NewLineCharSize.Width * 1.5f * GlobalScale.X;
            return measure;
        }

        protected override int GetSpriteCount()
        {
            if (IsPlaceholderVisible)
            {
                int count = _placeholderSegment.SpriteCount;
                if (IsShadowed && _placeholderSegment.SpriteCount > 0)
                    count++;
                return count;
            }
            return base.GetSpriteCount();
        }
        #endregion
    }
}
