﻿using System;
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
        private bool _shadowed;
        private TextHorizontalAlignment _horizontalAlignment;
        private TextVerticalAlignment _verticalAlignment;
        private QuadTree<CharSpriteInfo> _quadTree;

        public UITextElement(UIManager manager, BitmapFont font) : base(manager)
        {
            _font = font;
            _segment = new TextSegment(font);
            _segment.GlyphCallback = GlyphCallback;
            _quadTree = QuadTreePool<CharSpriteInfo>.Rent(RectangleF.Empty, threshold: 2, allowOverflow: true, fuzzyBoundaries: true);

            Color = Color.White;
            CaretColor = Color.Orange;
            HorizontalAlignment = TextHorizontalAlignment.Left;
            VerticalAlignment = TextVerticalAlignment.Top;
            
            IsShadowed = false;
            ShadowColor = new Color(Color.Black, 0.75f);
            ShadowSpacing = new ThicknessF(2, -1, 2, -1);

            IsMouseEventTrigger = true;
            OnMouseDown += UITextElement_OnMousePress;
        }

        // TODO: move into some other file
        private void UITextElement_OnMousePress(UIElement sender, MouseState mouseState, MouseButton buttons)
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
                SizeF size = glyph.Character == '\n' ?
                    GetNewLineCharSize() : new SizeF(glyph.FontRegion.Width * 0.4f, GetQuadTreeCharHeight());

                line = (int)Math.Floor(glyph.Position.Y / _font.LineHeight);
                var pos = new PointF(glyph.Position.X, line * _font.LineHeight + size.Height / 2f);

                rect = new RectangleF(pos, size);
                _quadTree.Insert(rect, new CharSpriteInfo(i, line));

                if(glyph.FontRegion != null)
                    rect.X += glyph.FontRegion.XAdvance;
            }

            // insert tail that's used to check if we're at the end
            if (glyphCount > 0)
            {
                var tailRect = new RectangleF(rect.Position, GetNewLineCharSize());
                _quadTree.Insert(tailRect, new CharSpriteInfo(glyphCount, line));
            }
        }

        public override void Draw(GameTime time, SpriteBatch batch)
        {
            if (IsShadowed)
                batch.DrawFilledRectangle(_boundaries, ShadowColor);
            batch.DrawString(_segment, _stringRect.Position);

            // TODO: move everything underneath into some other file
            if (!BuildQuadTree)
                return;

            var scale = GlobalScale;
            _ = new RectangleF(_stringRect.Position, SizeF.Empty);

            //DrawTree(batch, _quadTree, offset, scale);

            var posInTree = (Input.MousePosition.ToVector2() - _stringRect.Position.ToVector2()) / scale;
            var range = new RectangleF(posInTree - new Vector2(_font.LineHeight), new Vector2(_font.LineHeight * 2));
            
            var last = _quadTree.QueryNearest(range, posInTree);
            if (last.HasValue)
            {
                var quadItem = last.Value;
                if (IsSelected && quadItem.Value.Index >= 0 && _segment.SpriteCount > 0)
                {
                    float lineHeight = _font.LineHeight * scale.Y;
                    float lineY = quadItem.Value.Line * lineHeight + _stringRect.Y;

                    float caretWidth = 3f;
                    float caretHeight = lineHeight * 0.75f;
                    
                    var rect = new RectangleF(
                        quadItem.Bounds.X * scale.X + _stringRect.X - caretWidth,
                        lineY + lineHeight / 2f - caretHeight / 2f,
                        width: caretWidth,
                        height: caretHeight);
                    
                    batch.DrawFilledRectangle(rect, CaretColor);
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