using MonoGame.Extended.BitmapFonts;

namespace GeneralShare.UI
{
    public partial class UITextField : UITextElement
    {
        protected override void NeedsCleanup()
        {
            base.NeedsCleanup();

            if (MarkClean(DirtMarkType.PlaceholderColor))
            {
                _placeholderSegment.Color = _placeholderColor;
                _placeholderSegment.ApplyColors();
                return;
            }
        }

        protected override void BuildTextSprites(bool glyphUpdate)
        {
            base.BuildTextSprites(glyphUpdate);

            if (!glyphUpdate)
            {
                _placeholderSegment.Scale = Scale;
                _placeholderSegment.ClipRect = ClipRect;
                _placeholderSegment.BuildSprites(measure: true);
            }
        }

        //protected override RectangleF OnBoundaryUpdate(RectangleF newRect)
        //{
        //    return newRect;
        //}

        protected override TextSegment.GlyphCallbackResult GlyphCallback(ICharIterator source)
        {
            if (_isObscured)
                return new TextSegment.GlyphCallbackResult(_obscureChar.ToRepeatingIterator(source.Length), leaveOpen: false);

            if (!_isMultiLined)
            {
                var builder = StringBuilderPool.Rent(source.Length);
                for (int i = 0; i < source.Length; i++)
                {
                    char current = source.GetCharacter16(i);
                    if (current != '\n')
                        builder.Append(current);
                }

                var iter = builder.ToIterator();
                StringBuilderPool.Return(builder);
                return new TextSegment.GlyphCallbackResult(iter, leaveOpen: false);
            }

            return base.GlyphCallback(source);
        }
    }
}
