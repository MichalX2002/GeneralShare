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
        private RectangleF _stringRect;
        private RectangleF _boundaries;
        private TextAlignment _alignment;
        private bool _shadowed;
        private QuadTree<CharSpriteInfo> _quadTree;

        #region Properties
        protected SizeF NewLineCharSize => new SizeF(_font.LineHeight / 8f, _font.LineHeight / 2f);

        protected ICharIterator TextValue { get => _segment.CurrentText; set => SetTextValue(value); }
        protected abstract bool AllowTextColorFormatting { get; }
        public int Length => TextValue.Length;

        public override RectangleF Boundaries => GetBoundaries();
        public RectangleF StringRect => GetStringRect();
        public BitmapFont Font { get => _font; set => SetFont(value); }
        public Color Color { get => _segment.Color; set => SetColor(value); }
        public TextAlignment HorizontalAlignment { get => _alignment; set => SetAlignment(value); }
        public RectangleF? ClipRect { get => _segment.ClipRect; set => SetClipRect(value); }
        public bool BuildQuadTree { get; set; }

        public bool IsShadowed { get => _shadowed; set => SetIsShadowed(value); }
        public Color ShadowColor { get; set; }
        public ThicknessF ShadowSpacing { get; set; }
        #endregion

        public UITextElement(UIManager manager, BitmapFont font) : base(manager)
        {
            _font = font;
            _segment = new TextSegment(font);
            _segment.GlyphCallback = GlyphCallback;
            _quadTree = QuadTreePool<CharSpriteInfo>.Rent(RectangleF.Empty, threshold: 2, allowOverflow: true, fuzzyBoundaries: true);

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
            int glyphCount = _segment._glyphList.Count;
            if (glyphCount == 0)
                return;
            
            int line = default;
            RectangleF rect = default;

            _quadTree.Clear();
            for (int i = 0; i < glyphCount; i++)
            {
                BitmapFont.Glyph glyph = _segment._glyphList[i];
                if (glyph.FontRegion == null && glyph.Character != '\n')
                    continue;

                // the new line char may actually have a region
                SizeF size = (glyph.FontRegion == null && glyph.Character == '\n') ?
                    NewLineCharSize : new SizeF(glyph.FontRegion.Width / 2f, _font.LineHeight / 2f);

                line = (int)Math.Floor(glyph.Position.Y / _font.LineHeight);
                var pos = new PointF(glyph.Position.X + size.Width / 2f, line * _font.LineHeight + size.Height / 2f);

                rect = new RectangleF(pos, size);
                _quadTree.Insert(rect, new CharSpriteInfo(i, line));
            }

            if (glyphCount > 0)
            {
                RectangleF tailRect = rect;
                tailRect.X += rect.Width * 2f + _font.LetterSpacing;
                tailRect.Size = NewLineCharSize;

                _quadTree.Insert(tailRect, new CharSpriteInfo(glyphCount, line));
            }
        }

        public override void Draw(GameTime time, SpriteBatch batch)
        {
            if (IsShadowed)
                batch.DrawFilledRectangle(_boundaries, ShadowColor);
            batch.DrawString(_segment, _stringRect.Position);

            var pos = GlobalPosition;
            var scale = GlobalScale;
            var offset = new RectangleF(pos.ToVector2(), SizeF.Empty);
            
            //DrawTree(batch, _quadTree, offset, scale);
        
            var posInTree = (Input.MousePosition.ToVector2() - pos.ToVector2()) / scale;
            var range = new RectangleF(posInTree - new Vector2(_font.LineHeight), new Vector2(_font.LineHeight * 2));
            batch.DrawRectangle(new RectangleF(range.Position * scale, range.Size * scale) + offset, Color.Orange, 2);

            var _last = _quadTree.QueryNearest(range, posInTree);
            if (_last.HasValue)
            {
                var quadItem = _last.Value;
                if (IsSelected && quadItem.Value.Index >= 0 && _segment.SpriteCount > 0)
                {
                    PointF glyphPos;
                    if (quadItem.Value.Index < _segment._glyphList.Count)
                        glyphPos = _segment._glyphList[quadItem.Value.Index].Position;
                    else
                        glyphPos = quadItem.Bounds.Position;
                    glyphPos *= scale;

                    var rect = new RectangleF(
                        glyphPos.X,
                        quadItem.Value.Line * _font.LineHeight * scale.Y,
                        width: 4f,
                        height: _font.LineHeight * scale.Y * 0.75f);

                    rect.X -= rect.Width;
                    rect.Y += _font.LineHeight * scale.Y * 0.25f / 2f;

                    batch.DrawFilledRectangle(rect + offset, Color.OrangeRed);
                }
            }

            batch.DrawRectangle(offset + new RectangleF(0, 0, _boundaries.Width, _font.LineHeight * scale.Y), Color.White, 2);
            
            batch.DrawRectangle(_stringRect, Color.LimeGreen, 2);
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

        public readonly struct CharSpriteInfo
        {
            public int Index { get; }
            public int Line { get; }

            public CharSpriteInfo(int index, int line)
            {
                Index = index;
                Line = line;
            }
        }
    }
}