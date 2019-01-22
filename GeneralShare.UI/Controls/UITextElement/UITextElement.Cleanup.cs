using Microsoft.Xna.Framework;
using MonoGame.Extended.BitmapFonts; 

namespace GeneralShare.UI
{
    public abstract partial class UITextElement
    {
        protected override void NeedsCleanup()
        {
            if (HasDirtMarks(DirtMarkType.Color))
            {
                _segment.ApplyColors();
                if (MarkClean(DirtMarkType.Color))
                    return;
            }

            if (MarkClean(
                DirtMarkType.Position |
                DirtMarkType.Shadowed | 
                DirtMarkType.ShadowSpacing |
                DirtMarkType.TextAlignment))
            {
                UpdateBoundaries();
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

        protected void UpdateGlyphs()
        {
            _segment.UpdateGlyphs();
            FullCleanup();

            InvokeMarkedDirty(DirtMarkType.Boundaries);
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
            _stringRect = OnStringRectUpdate(
                new RectangleF(GlobalPosition.ToVector2() + CalculateAlignmentOffset(), GetMeasure()));
            
            _boundaries = OnBoundaryUpdate(_stringRect);

            if (BuildQuadTree && Length > 0)
            {
                // TODO: fix quad tree, the float arithmetic is incorrect and we need some offset here
                //       or the items will be "out of bounds" (we use fuzzy boundaries as a remedy though)
                var offset = new RectangleF(0, 0, NewLineCharSize.Width, 0);
                _quadTree.Resize(new RectangleF(PointF.Zero, GetMeasure() / _segment.Scale) + offset);
            }
        }

        protected virtual void BuildTextSprites()
        {
            _segment.Scale = GlobalScale;
            _segment.BuildSprites(measure: true);
        }

        protected virtual SizeF GetMeasure()
        {
            return _segment.Measure;
        }

        protected virtual RectangleF OnStringRectUpdate(RectangleF newRect)
        {
            if (IsShadowed)
            {
                // TODO: fix ambiguity error in framework resulting in a unnecessary cast
                newRect.Position -= (Vector2)ShadowSpacing.ToOffsetPosition(GlobalScale);
            }

            return newRect;
        }

        protected virtual RectangleF OnBoundaryUpdate(RectangleF newRect)
        {
            //TODO: fix offsets

            if (IsShadowed)
                newRect += ShadowSpacing.ToOffsetRectangle(GlobalScale);
           
            return newRect;
        }

        protected virtual TextSegment.GlyphCallbackResult GlyphCallback(ICharIterator source)
        {
            return new TextSegment.GlyphCallbackResult(source, leaveOpen: true);
        }
    }
}
