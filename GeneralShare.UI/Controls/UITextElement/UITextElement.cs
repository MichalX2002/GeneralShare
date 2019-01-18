using System;
using GeneralShare.Collections;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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

        #region Properties
        protected Vector2 StartPosition { get; private set; }
        protected bool IsShadowVisisble => IsShadowed && GetSpriteCount() > 0;

        protected abstract bool AllowTextColorFormatting { get; }
        protected ICharIterator TextValue { get => _segment.CurrentText; set => SetTextValue(value); }

        public override RectangleF Boundaries => GetBounds();
        public BitmapFont Font { get => _font; set => SetFont(value); }
        public Color Color { get => _segment.Color; set => SetColor(value); }
        public TextAlignment HorizontalAlignment { get => _alignment; set => SetAlignment(value); }
        public RectangleF? ClipRect { get => _segment.ClipRect; set => SetClipRect(value); }

        public SizeF Measure => GetMeasure();
        public int Length => TextValue.Length;
        public int SpriteCount => GetSpriteCount();

        public bool IsShadowed { get; set; }
        public Color ShadowColor { get; set; }
        public ThicknessF ShadowSpacing { get; set; }
        #endregion

        public UITextElement(UIManager manager, BitmapFont font) : base(manager)
        {
            _font = font;
            _segment = new TextSegment(font);
            _segment.GlyphCallback = GlyphCallback;

            Color = Color.White;
            HorizontalAlignment = TextAlignment.Left;

            IsMouseEventTrigger = true;

            IsShadowed = false;
            ShadowColor = new Color(Color.Black, 0.75f);
            ShadowSpacing = new ThicknessF(2, -1, 2, -1);

            OnMouseDown += UITextElement_OnMousePress;
        }

        private void UITextElement_OnMousePress(Microsoft.Xna.Framework.Input.MouseState mouseState, MouseButton buttons)
        {
            if (_segment._glyphList.Count == 0)
            {
                _last = new QuadTree<int>.Item(default, -1);
                return;
            }

            _tree.ClearTree();
            
            for (int i = 0; i < _segment._glyphList.Count; i++)
            {
                BitmapFont.Glyph glyph = _segment._glyphList[i];
                if (glyph.FontRegion == null)
                    continue;
                
                var size = new SizeF(glyph.FontRegion.Width, _font.LineHeight / 2f);
                
                int line = (int)Math.Floor(glyph.Position.Y / _font.LineHeight);
                var pos = new PointF(glyph.Position.X, line * _font.LineHeight + size.Height / 2f);

                var rect = new RectangleF(pos, size);
                _tree.CurrentTree.Insert(rect, i);
            }

            var posInTree = (mouseState.Position.ToVector2() - GlobalPosition.ToVector2()) / GlobalScale;
            _range = new RectangleF(posInTree - new Vector2(_font.LineHeight), new Vector2(_font.LineHeight * 2));

            _last = _tree.CurrentTree.QueryNearest(_range, posInTree);
        }

        public PooledQuadTree<int> _tree = new PooledQuadTree<int>(2, true);
        RectangleF _range;
        QuadTree<int>.Item _last = new QuadTree<int>.Item(default, -1);

        public override void Draw(GameTime time, SpriteBatch batch)
        {
            if (IsShadowVisisble)
                batch.DrawFilledRectangle(_boundaries, ShadowColor);
            batch.DrawString(_segment, StartPosition);

            var scale = GlobalScale;
            var offset = new RectangleF(GlobalPosition.ToVector2(), SizeF.Empty);


            //batch.DrawRectangle(Boundaries, new Color(Color.Red, 0.666f), 1);

            void DrawTree(QuadTree<int> ree)
            {
                batch.DrawRectangle(new RectangleF(ree.Boundary.Position * scale, ree.Boundary.Size * scale) + offset, Color.Blue);
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
                    DrawTree(ree.TopLeft);
                    DrawTree(ree.TopRight);
                    DrawTree(ree.BottomLeft);
                    DrawTree(ree.BottomRight);
                }
            }

            DrawTree(_tree.CurrentTree);
            batch.DrawRectangle(new RectangleF(_range.Position * scale, _range.Size * scale) + offset, Color.Orange, 2);
            
            if (IsSelected && _last.Value >= 0 && _segment.SpriteCount > 0)
            {
                RectangleF rect = new RectangleF(_last.Bounds.Position * scale, _last.Bounds.Size * scale) + offset + new RectangleF(-2, -2, 4, 4);
                batch.DrawRectangle(rect, Color.OrangeRed, 3);
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
    }
}