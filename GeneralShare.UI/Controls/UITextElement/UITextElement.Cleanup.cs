using Microsoft.Xna.Framework;
using MonoGame.Extended.BitmapFonts; 

namespace GeneralShare.UI
{
    public abstract partial class UITextElement
    {
        protected override void NeedsCleanup()
        {
            if (MarkClean(
                DirtMarkType.Position |
                DirtMarkType.Shadowed | 
                DirtMarkType.ShadowSpacing |
                DirtMarkType.TextAlignment))
            {
                UpdateBoundaries();
                return;
            }

            if (MarkClean(DirtMarkType.Color))
            {
                _segment.ApplyColors();
                return;
            }

            //System.Console.WriteLine("Full Cleanup: " + DirtMarks);

            FullCleanup(glyphUpdate: false);
        }

        private void FullCleanup(bool glyphUpdate)
        {
            // mark clean first to prevent stack overflow
            MarkClean();

            BuildTextSprites(glyphUpdate);
            UpdateBoundaries();
        }

        private Vector2 CalculateAlignmentOffset()
        {
            switch (HorizontalAlignment)
            {
                default:
                    //case TextAlignment.Left:
                    return Vector2.Zero;

                case TextAlignment.Center:
                    return -new Vector2(GetMeasure().Width, 0) / 2f;

                case TextAlignment.Right:
                    return -new Vector2(GetMeasure().Width, 0);
            }
        }

        private void UpdateBoundaries()
        {
            StartPosition = GlobalPosition.ToVector2() + CalculateAlignmentOffset();
            _boundaries = new RectangleF(StartPosition, GetMeasure());

            if (IsShadowVisisble)
                _boundaries += ShadowSpacing.ToOffsetRectangle(GlobalScale);

            _boundaries = OnBoundaryUpdate(_boundaries);
        }

        protected virtual void BuildTextSprites(bool glyphUpdate)
        {
            _segment.Scale = GlobalScale;
            _segment.BuildSprites(measure: true);
        }

        protected virtual RectangleF OnBoundaryUpdate(RectangleF newRect)
        {
            return newRect;
        }

        protected virtual TextSegment.GlyphCallbackResult GlyphCallback(ICharIterator source)
        {
            return new TextSegment.GlyphCallbackResult(source, leaveOpen: true);
        }

        protected void UpdateGlyphs()
        {
            _segment.UpdateGlyphs();
            FullCleanup(glyphUpdate: true);
            InvokeMarkedDirty(DirtMarkType.Boundaries);
        }
    }
}
