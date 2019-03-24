using Microsoft.Xna.Framework;
using MonoGame.Extended.BitmapFonts; 

namespace GeneralShare.UI
{
    public abstract partial class UITextElement
    {
        protected override void Cleanup()
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

            UpdateQuadTreeBoundaries();
        }

        private void UpdateQuadTreeBoundaries()
        {
            if (BuildQuadTree)
            {
                if (Length > 0)
                {
                    // TODO: fix quad tree, the float arithmetic is incorrect and we often need some offset or
                    //       the items will be "out of bounds" (we can use fuzzy boundaries as a remedy though)

                    var bounds = new RectangleF(PointF.Zero, _stringRect.Size / GlobalScale);
                    bounds.Width += GetNewLineCharSize().Width * 2; // to ensure that the last quad will fit
                    _quadTree.Resize(bounds);

                    return;
                }
            }

            // clear if don't build the quad tree or if length was zero
            _quadTree.Clear();
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
                newRect.Position -= ShadowSpacing.ToOffsetPosition(GlobalScale);

            newRect.X += GetTextOffsetX();

            return newRect;
        }

        protected virtual RectangleF OnBoundaryUpdate(RectangleF newRect)
        {
            if (IsShadowed)
                newRect += ShadowSpacing.ToOffsetRectangle(GlobalScale);

            float textOffset = GetTextOffsetX();
            newRect.X -= textOffset;
            newRect.Width += textOffset * 2;

            return newRect;
        }

        protected virtual TextSegment.GlyphCallbackResult GlyphCallback(ICharIterator source)
        {
            return new TextSegment.GlyphCallbackResult(source, leaveOpen: true);
        }
    }
}
