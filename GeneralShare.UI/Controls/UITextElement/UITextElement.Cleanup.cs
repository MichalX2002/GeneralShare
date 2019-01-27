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

        private void UpdateBoundaries()
        {
            _stringRect = OnStringRectUpdate(
                new RectangleF(GlobalPosition.ToVector2() + GetAlignmentOffset(), GetMeasure()));
            
            _boundaries = OnBoundaryUpdate(_stringRect);

            if (BuildQuadTree && Length > 0)
            {
                // TODO: fix quad tree, the float arithmetic is incorrect and we need some offset here
                //       or the items will be "out of bounds" (we use fuzzy boundaries as a remedy though)
                var bounds = new RectangleF(PointF.Zero, _stringRect.Size / GlobalScale);
                bounds.Width += GetNewLineCharSize().Width; // just to add some extra space
                _quadTree.Resize(bounds);
            }
        }

        protected virtual void BuildTextSprites()
        {
            _segment.Scale = GlobalScale;
            _segment.BuildSprites(measure: true);
        }

        protected virtual RectangleF OnStringRectUpdate(RectangleF newRect)
        {
            if (IsShadowed)
                // TODO: fix ambiguity error in monogameframework resulting in a unnecessary cast
                newRect.Position -= (Vector2)ShadowSpacing.ToOffsetPosition(GlobalScale);

            return newRect;
        }

        protected virtual RectangleF OnBoundaryUpdate(RectangleF newRect)
        {
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
