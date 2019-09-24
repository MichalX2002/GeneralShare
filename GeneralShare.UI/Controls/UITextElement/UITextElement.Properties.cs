using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.BitmapFonts;

namespace GeneralShare.UI
{
    public abstract partial class UITextElement : UIElement
    {
        #region Properties
        protected ICharIterator TextValue { get => _segment.CurrentText; set => SetTextValue(value); }
        protected abstract bool AllowTextColorFormatting { get; }
        public int Length => TextValue.Length;

        public override RectangleF Boundaries => GetBoundaries();
        public RectangleF StringRect => GetStringRect();
        public SizeF Measure => StringRect.Size;

        public BitmapFont Font { get => _font; set => SetFont(value); }
        public Color Color { get => _segment.Color; set => SetColor(value); }
        public TextHorizontalAlignment HorizontalAlignment { get => _horizontalAlignment; set => SetHorizontalAlignment(value); }
        public TextVerticalAlignment VerticalAlignment { get => _verticalAlignment; set => SetVerticalAlignment(value); }
        public RectangleF? ClipRect { get => _segment.ClipRect; set => SetClipRect(value); }

        public bool BuildQuadTree { get; set; }
        public Color CaretColor { get; }

        public bool IsShadowed { get => _shadowed; set => SetIsShadowed(value); }
        public Color ShadowColor { get; set; }
        public ThicknessF ShadowSpacing { get; set; }
        #endregion

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

        private void SetHorizontalAlignment(TextHorizontalAlignment value)
        {
            MarkDirty(ref _horizontalAlignment, value, DirtMarkType.TextAlignment);
        }

        private void SetVerticalAlignment(TextVerticalAlignment value)
        {
            MarkDirty(ref _verticalAlignment, value, DirtMarkType.TextAlignment);
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
            Purify();
            return _boundaries;
        }

        protected virtual RectangleF GetStringRect()
        {
            Purify();
            return _stringRect;
        }

        protected float GetQuadTreeCharHeight()
        {
            return _font.LineHeight / 2f;
        }

        protected SizeF GetNewLineCharSize()
        {
            float width = _font.GetCharacterRegion('\n', out var region) ? region.Width : 1;
            float height = GetQuadTreeCharHeight();
            return new SizeF(width, height);
        }

        private float GetTextOffsetX()
        {
            return GetNewLineCharSize().Width * GlobalScale.X;
        }

        protected virtual SizeF GetMeasure()
        {
            return _segment.Measure;
        }
        
        protected Vector2 GetAlignmentOffset()
        {
            var offset = Vector2.Zero;
            var measure = GetMeasure() + ShadowSpacing.ToOffsetSize(GlobalScale);
            measure.Width += GetTextOffsetX() * 2;

            switch (HorizontalAlignment)
            {
                case TextHorizontalAlignment.Center:
                    offset.X = -measure.Width / 2f;
                    break;

                case TextHorizontalAlignment.Right:
                    offset.X = -measure.Width;
                    break;
            }

            switch (VerticalAlignment)
            {
                case TextVerticalAlignment.Center:
                    offset.Y = -measure.Height / 2f;
                    break;

                case TextVerticalAlignment.Bottom:
                    offset.Y = -measure.Height;
                    break;
            }

            return offset;
        }
        #endregion
    }
}
