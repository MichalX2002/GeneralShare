using System;
using GeneralShare.Collections;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.BitmapFonts;

namespace GeneralShare.UI
{
    public abstract partial class UITextElement : UIElement
    {
        private BitmapFont _font;
        private TextSegment _segment;
        private RectangleF _boundaries;
        private TextAlignment _alignment;
        private QuadTree<int> _quadTree;

        #region Properties
        protected Vector2 StartPosition { get; private set; }
        protected bool IsShadowVisisble => IsShadowed && GetSpriteCount() > 0;
        protected SizeF NewLineCharSize => new SizeF(_font.LineHeight / 8f, _font.LineHeight / 2f);

        protected ICharIterator TextValue { get => _segment.CurrentText; set => SetTextValue(value); }
        protected abstract bool AllowTextColorFormatting { get; }

        public SizeF Measure => GetMeasure();
        public int Length => TextValue.Length;
        public int SpriteCount => GetSpriteCount();

        public override RectangleF Boundaries => GetBounds();
        public BitmapFont Font { get => _font; set => SetFont(value); }
        public Color Color { get => _segment.Color; set => SetColor(value); }
        public TextAlignment HorizontalAlignment { get => _alignment; set => SetAlignment(value); }
        public RectangleF? ClipRect { get => _segment.ClipRect; set => SetClipRect(value); }
        public bool BuildQuadTree { get; set; }

        public bool IsShadowed { get; set; }
        public Color ShadowColor { get; set; }
        public ThicknessF ShadowSpacing { get; set; }
        #endregion

        public UITextElement(UIManager manager, BitmapFont font) : base(manager)
        {
            _font = font;
            _segment = new TextSegment(font);
            _segment.GlyphCallback = GlyphCallback;
            _quadTree = QuadTreePool<int>.Rent(RectangleF.Empty, 2, allowOverflow: true, fuzzyBoundaries: true);

            Color = Color.White;
            HorizontalAlignment = TextAlignment.Left;
            IsMouseEventTrigger = true;
            BuildQuadTree = true;

            IsShadowed = false;
            ShadowColor = new Color(Color.Black, 0.75f);
            ShadowSpacing = new ThicknessF(2, -1, 2, -1);
            
            OnMouseDown += UITextElement_OnMousePress;
        }

        private void UITextElement_OnMousePress(MouseState mouseState, MouseButton buttons)
        {
            var list = _segment._glyphList;
            int glyphCount = list.Count;
            if (glyphCount == 0)
                return;

            BitmapFont.Glyph lastGlyph = default;
            RectangleF lastRect = default;

            _quadTree.Clear();
            for (int i = 0; i < glyphCount; i++)
            {
                lastGlyph = list[i];
                if (lastGlyph.FontRegion == null && lastGlyph.Character != '\n')
                    continue;

                var size = lastGlyph.Character == '\n' ?
                    NewLineCharSize : new SizeF(lastGlyph.FontRegion.Width / 2f, _font.LineHeight / 2f);

                int line = (int)Math.Floor(lastGlyph.Position.Y / _font.LineHeight);
                var pos = new PointF(lastGlyph.Position.X + size.Width / 2f, line * _font.LineHeight + size.Height / 2f);

                lastRect = new RectangleF(pos, size);
                _quadTree.Insert(lastRect, i);
            }

            if (glyphCount > 0)
            {
                RectangleF tailRect = lastRect;
                tailRect.X += lastRect.Width * 2f + _font.LetterSpacing;
                tailRect.Size = NewLineCharSize;

                _quadTree.Insert(tailRect, glyphCount);
            }
        }

        public override void Draw(GameTime time, SpriteBatch batch)
        {
            if (IsShadowVisisble)
                batch.DrawFilledRectangle(_boundaries, ShadowColor);
            batch.DrawString(_segment, StartPosition);

            var scale = GlobalScale;
            var offset = new RectangleF(GlobalPosition.ToVector2(), SizeF.Empty);
            
            //DrawTree(batch, _quadTree, offset, scale);
        
            var posInTree = (Input.MousePosition.ToVector2() - GlobalPosition.ToVector2()) / GlobalScale;
            RectangleF _range = new RectangleF(posInTree - new Vector2(_font.LineHeight), new Vector2(_font.LineHeight * 2));
            batch.DrawRectangle(new RectangleF(_range.Position * scale, _range.Size * scale) + offset, Color.Orange, 2);

            var _last = _quadTree.QueryNearest(_range, posInTree);
            if (_last.HasValue)
            {
                var item = _last.Value;
                if (IsSelected && item.Value >= 0 && _segment.SpriteCount > 0)
                {
                    RectangleF rect = new RectangleF(item.Bounds.Position * scale, item.Bounds.Size * scale) + offset + new RectangleF(-2, -2, 4, 4);
                    batch.DrawRectangle(rect, Color.OrangeRed, 3);
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_segment != null)
                {
                    _segment.Dispose();
                    _segment = null;
                }
            }

            base.Dispose(disposing);
        }

        private void DrawTree<T>(SpriteBatch batch, QuadTree<T> ree, RectangleF offset, Vector2 scale)
        {
            batch.DrawRectangle(new RectangleF(ree.Bounds.Position * scale, ree.Bounds.Size * scale) + offset, Color.Blue);
            foreach (var rect in ree.Items)
            {
                var r = new RectangleF(rect.Bounds.Position * scale, rect.Bounds.Size * scale);
                if (r.Width != 0 && r.Height != 0)
                    batch.DrawRectangle(r + offset, Color.Green);
                else
                    batch.DrawPoint(r.Position.ToVector2() + offset.Position, Color.Green, 2);
            }
            if (ree.IsDivided)
            {
                DrawTree(batch, ree.TopLeft, offset, scale);
                DrawTree(batch, ree.TopRight, offset, scale);
                DrawTree(batch, ree.BottomLeft, offset, scale);
                DrawTree(batch, ree.BottomRight, offset, scale);
            }
        }
    }
}