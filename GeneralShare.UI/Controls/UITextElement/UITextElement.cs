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
            _last = _segment.FindNearestGlyphIndex(mouseState.Position - StartPosition.ToPoint());
        }

        int _last = -1;

        public override void Draw(GameTime time, SpriteBatch batch)
        {
            if (IsShadowVisisble)
                batch.DrawFilledRectangle(_boundaries, ShadowColor);
            batch.DrawString(_segment, StartPosition);

            if (IsSelected && _last >= 0 && _segment.SpriteCount > 0)
            {
                RectangleF rect;
                if (_last >= _segment.SpriteCount)
                {
                    var g = _segment.GetSprite(_segment.SpriteCount - 1);
                    rect = new RectangleF(
                           StartPosition + new Vector2((g.Position - g.Origin).X + 5, 0) * GlobalScale,
                           new SizeF(5, Font.LineHeight) * GlobalScale);
                }
                else
                {
                    var g = _segment.GetSprite(_last);
                    rect = new RectangleF(
                        StartPosition + g.Position - g.Origin * GlobalScale,
                        g.SourceRect.Size * GlobalScale);
                }

                batch.DrawRectangle(rect, Color.LightGreen, 2);
            }

            //batch.DrawRectangle(Boundaries, new Color(Color.Red, 0.666f), 1);
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