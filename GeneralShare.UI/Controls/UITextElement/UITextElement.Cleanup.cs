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

            FullCleanup();
        }

        private void FullCleanup()
        {
            // mark clean first to prevent stack overflow
            MarkClean();

            BuildTextSprites();
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

            if (BuildQuadTree)
            {
                // TODO: fix quad tree, the float arithmetic is incorrect and we need some offset here
                //       or the items will be "out of bounds" (we use fuzzy boundaries as a remedy though)
                _quadTree.Resize(new RectangleF(PointF.Zero, GetMeasure() / _segment.Scale));
            }
        }

        protected virtual void BuildTextSprites()
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
            FullCleanup();
            InvokeMarkedDirty(DirtMarkType.Boundaries);
        }
    }
}
