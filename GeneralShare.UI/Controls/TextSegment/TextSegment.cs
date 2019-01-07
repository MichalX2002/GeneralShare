using GeneralShare.Collections;
using Microsoft.Xna.Framework;
using MonoGame.Extended.BitmapFonts;
using System;

namespace GeneralShare.UI
{
    public partial class TextSegment : IDisposable
    {
        public delegate GlyphCallbackResult GlyphCallbackDelegate(ICharIterator source);
        public static readonly GlyphCallbackDelegate DefaultGlyphCallback;

        private BitmapFont _font;
        private bool _usedColorFormat;
        private GlyphCallbackDelegate _glyphCallback;

        private ListArray<BitmapFont.Glyph> _glyphList;
        private ListArray<Color?> _formatColorList;
        internal ListArray<GlyphSprite> _spriteList;

        public bool IsDisposed { get; private set; }
        public ICharIterator CurrentText { get; private set; }
        public Color Color { get; set; }
        public Vector2 Scale { get; set; }
        public RectangleF? ClipRect { get; set; }

        public int SpriteCount => _spriteList.Count;
        public SizeF Measure { get; private set; }

        public GlyphCallbackDelegate GlyphCallback
        {
            get => _glyphCallback;
            set => _glyphCallback = value ?? throw new ArgumentNullException(nameof(value));
        }

        static TextSegment()
        {
            DefaultGlyphCallback = (iter) => new GlyphCallbackResult(iter, true); // just return the source
        }

        public TextSegment(BitmapFont font)
        {
            _font = font ?? throw new ArgumentNullException(nameof(font));

            _glyphList = new ListArray<BitmapFont.Glyph>();
            _formatColorList = new ListArray<Color?>();
            _spriteList = new ListArray<GlyphSprite>();

            GlyphCallback = DefaultGlyphCallback;
            CurrentText = EmptyCharIterator.Instance;
            Color = Color.White;
            Scale = Vector2.One;
        }

        public void SetText(BitmapFont font, ICharIterator chars, bool useColorFormat)
        {
            _font = font ?? throw new ArgumentNullException(nameof(font));

            _usedColorFormat = useColorFormat;
            _formatColorList.Clear();
            _glyphList.Clear();

            if (chars.Length == 0)
            {
                CurrentText = EmptyCharIterator.Instance;
            }
            else
            {
                void GetGlyphs(ICharIterator sourceIter)
                {
                    GlyphCallbackResult result = _glyphCallback.Invoke(sourceIter);
                    Measure = _font.GetGlyphs(result.Iterator, _glyphList) * Scale;
                    ApplyClipRect();

                    if (!result.LeaveOpen)
                        result.Iterator.Dispose();
                }

                if (useColorFormat)
                {
                    using (var result = TextRenderer.GetColorFormat(_font, chars, keepSequences: false, _formatColorList))
                        GetGlyphs(result);
                }
                else
                    GetGlyphs(chars);

                var builder = StringBuilderPool.Rent(chars.Length);
                builder.AppendIterator(chars);

                CurrentText?.Dispose();
                CurrentText = builder.ToIterator();

                // we can return as ToIterator() copies the value into it's own buffer
                StringBuilderPool.Return(builder);
            }

            BuildSprites(measure: false);
        }

        public void UpdateGlyphs()
        {
            SetText(_font, CurrentText, _usedColorFormat);
        }

        public void BuildSprites(bool measure)
        {
            _spriteList.Clear();

            if (_font == null || _glyphList.Count == 0 || CurrentText.Length == 0)
            {
                Measure = SizeF.Empty;
                return;
            }
            
            BitmapFontExtensions.GetGlyphSprites(
                _glyphList, _spriteList, Vector2.Zero, Color, 0, Vector2.Zero, Scale, 0, ClipRect);

            if (measure)
                Measure = _font.MeasureString(_glyphList) * Scale;
            ApplyClipRect();

            // don't apply base color as GetGlyphSprites() already does
            ApplyColors(applyBaseColor: false);
        }

        private void ApplyClipRect()
        {
            if (ClipRect.HasValue)
            {
                RectangleF clip = ClipRect.Value;
                Measure = Mathf.Clamp(Measure, SizeF.Empty, clip.Size);
            }
        }

        private void ApplyColors(bool applyBaseColor)
        {
            if (applyBaseColor)
            {
                for (int i = 0; i < _spriteList.Count; i++)
                    _spriteList.GetReferenceAt(i).Color = Color;
            }

            // apply the colors from the formatting
            for (int i = 0; i < _formatColorList.Count; i++)
            {
                Color? nullable = _formatColorList[i];
                if (nullable.HasValue)
                    _spriteList.GetReferenceAt(i).Color = nullable.Value;
            }
        }

        public void ApplyColors()
        {
            ApplyColors(applyBaseColor: true);
        }
        
        public BitmapFont.Glyph GetGlyph(int index)
        {
            return _glyphList[index];
        }

        public GlyphSprite GetSprite(int index)
        {
            return _spriteList[index];
        }

        public int FindNearestGlyphIndex(PointF position)
        {
            if (_glyphList.Count == 0)
                return -1;

            int index = 0;
            
            for (int g = 0; g < _glyphList.Count; g++)
            {
                BitmapFont.Glyph glyph = _glyphList[g];
                float fontRegionWidth = (glyph.FontRegion?.Width ?? 0) * Scale.X;
                float glyphMiddle = glyph.Position.X * Scale.X + fontRegionWidth * 0.5f;

                if (position.X >= glyphMiddle)
                {
                    index++;
                    continue;
                }
                return index;
            }

            return index;
        }

        public void Dispose()
        {
            if (!IsDisposed)
            {
                // TODO: recycle lists

                if (CurrentText != null)
                {
                    CurrentText.Dispose();
                    CurrentText = null;
                }

                IsDisposed = true;
            }
        }

        public readonly struct GlyphCallbackResult
        {
            public ICharIterator Iterator { get; }
            public bool LeaveOpen { get; }

            public GlyphCallbackResult(ICharIterator iterator, bool leaveOpen)
            {
                Iterator = iterator;
                LeaveOpen = leaveOpen;
            }
        }
    }
}