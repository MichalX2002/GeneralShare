using GeneralShare.Collections;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.BitmapFonts;
using MonoGame.Extended.TextureAtlases;
using System;
using System.Text;

namespace GeneralShare.UI
{
    public abstract class UITextBase : UIElement
    {
        private const DirtMarkType NEEDS_PROCESSING_MARKS =
            DirtMarkType.Color | DirtMarkType.Value | DirtMarkType.UseTextExpressions |
            DirtMarkType.Font | DirtMarkType.InputPrefix | DirtMarkType.KeepTextExpressions;

        private BitmapFont _font;
        private ListArray<BitmapFont.Glyph> _textGlyphs;
        protected string _value;
        protected StringBuilder _processedText;
        private ListArray<LineInfo> _lines;
        private bool _buildCharQuadTree;
        protected PooledQuadTree<float> _charQuadTree;

        protected ListArray<GlyphBatchedSprite> _textSprites;
        protected ListArray<Color> _expressionColors;

        protected bool _valueExpressions;
        protected bool _keepExpressions;
        private Color _color;
        private Rectangle? _clipRect;
        private RectangleF _textBounds;
        private RectangleF _totalBoundaries;
        private TextAlignment _align;
        private float _alignOffsetX;

        private bool _useShadow;
        private BatchedSprite _shadowSprite;
        private TextureRegion2D _shadowTex;
        private RectangleF _shadowOffset;
        private Point _shadowTexSrc;
        private Color _shadowColor;
        private bool _shadowAvailable;

        public Color BaseColor { get => _color; set => SetColor(value); }
        public BitmapFont Font { get => _font; set => SetFont(value); } // add fontscale that scales with font.LineHeight
        public Rectangle? ClippingRectangle { get => _clipRect; set => SetClipRect(value); }
        public int SpecialProcessedTextLength { get; private set; }

        public bool UseShadow { get => _useShadow; set => SetUseShadow(value); }
        public Color ShadowColor { get => _shadowColor; set => SetShadowColor(value); }
        public TextureRegion2D ShadowTexture { get => _shadowTex; set => SetShadowTex(value); }
        public RectangleF ShadowOffset { get => _shadowOffset; set => SetShadowOffset(value); }

        public bool BuildCharQuadTree { get => _buildCharQuadTree; set => SetBuildCharQuadTree(value); }
        public ReadOnlyQuadTree<float> CharQuadTree => _charQuadTree.CurrentTree.AsReadOnly();
        public override RectangleF Boundaries { get { ProcessTextIfNeeded(); return _totalBoundaries; } }
        public RectangleF TextBoundaries { get { ProcessTextIfNeeded(); return _textBounds; } }
        public SizeF Measure => TextBoundaries.Size;
        public TextAlignment Alignment { get => _align; set => SetAlign(value); }

        public UITextBase(UIManager manager, BitmapFont font) : base(manager)
        {
            _value = string.Empty;
            _textGlyphs = new ListArray<BitmapFont.Glyph>();
            _processedText = new StringBuilder();
            _charQuadTree = new PooledQuadTree<float>(Rectangle.Empty, 2, true);
            _lines = new ListArray<LineInfo>();

            _textSprites = new ListArray<GlyphBatchedSprite>();
            _expressionColors = new ListArray<Color>();

            Font = font;
            BaseColor = Color.White;
            Scale = Vector2.One;

            SetShadowRadius(5, 0);
            ShadowColor = new Color(Color.Gray, 0.5f);
            if (manager != null)
                ShadowTexture = manager.WhitePixelRegion;
        }

        public UITextBase(BitmapFont font) : this(null, font)
        {
        }

        public string GetProcessedText()
        {
            ProcessTextIfNeeded();
            return _processedText.ToString();
        }

        protected void SetAlign(TextAlignment align)
        {
            MarkDirty(ref _align, align, DirtMarkType.TextAlignment);
        }

        protected void SetBuildCharQuadTree(bool value)
        {
            MarkDirtyE(ref _buildCharQuadTree, value, DirtMarkType.BuildQuadTree);
        }

        public void SetShadowRadius(float radius)
        {
            SetShadowOffset(new RectangleF(-radius, -radius, radius * 2, radius * 2));
        }

        public void SetShadowRadius(float width, float height)
        {
            SetShadowOffset(new RectangleF(-width, -height, width * 2, height * 2));
        }

        protected void SetShadowOffset(RectangleF value)
        {
            value.X -= 1;
            value.Width += 1;
            MarkDirtyE(ref _shadowOffset, value, DirtMarkType.ShadowOffset);
        }

        protected void SetShadowColor(Color value)
        {
            MarkDirtyE(ref _shadowColor, value, DirtMarkType.ShadowColor);
        }

        protected void SetShadowTex(TextureRegion2D value)
        {
            _shadowTex = value;
            _shadowTexSrc = value.Bounds.Size;
        }

        protected void SetUseShadow(bool value)
        {
            MarkDirty(ref _useShadow, value, DirtMarkType.UseShadow);
        }

        protected void SetFont(BitmapFont value)
        {
            MarkDirty(ref _font, value, DirtMarkType.Font);
        }

        protected void SetValueExp(bool value)
        {
            MarkDirtyE(ref _valueExpressions, value, DirtMarkType.UseTextExpressions);
        }

        protected void SetKeepExp(bool value)
        {
            MarkDirtyE(ref _keepExpressions, value, DirtMarkType.KeepTextExpressions);
        }

        protected void SetValue(string value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            MarkDirty(ref _value, value, DirtMarkType.Value);
        }

        protected void SetColor(Color value)
        {
            MarkDirtyE(ref _color, value, DirtMarkType.Color);
        }

        protected void SetClipRect(Rectangle? value)
        {
            MarkDirty(ref _clipRect, value, DirtMarkType.ClipRectangle);
        }

        public void BuildGraphicsIfNeeded()
        {
            if (Dirty == false)
                return;

            ProcessTextIfNeeded();
            lock (SyncRoot)
            {
                bool hadDirtyGraphics = HasAnyDirtMark(
                    DirtMarkType.ValueProcessed | DirtMarkType.Transform |
                    DirtMarkType.Font | DirtMarkType.ClipRectangle);
                if (hadDirtyGraphics)
                {
                    Vector3 pos = GlobalPosition;
                    _textBounds.X = pos.X + _alignOffsetX;
                    _textBounds.Y = pos.Y;

                    if (_processedText.Length > 0 && _font != null)
                    {
                        _textSprites.Clear(false);
                        BitmapFontExtensions.GetGlyphSprites(
                            _textGlyphs, _textSprites, pos.ToVector2(), _color, Rotation, Origin, Scale, Z, _clipRect);

                        if (_buildCharQuadTree)
                            PrepareQuadTree();
                        
                        for (int i = 0; i < _textSprites.Count; i++)
                        {
                            ref var sprite = ref _textSprites.GetReferenceAt(i);
                            if (_buildCharQuadTree)
                                AddTextQuadItem(ref pos, ref sprite);

                            if (_valueExpressions && i < _expressionColors.Count)
                                sprite.SetColor(_expressionColors[i]);
                        }

                        MarkDirty(DirtMarkType.ShadowMath, true);
                    }
                    else
                        _textSprites.Clear(false);
                }

                if (DirtMarks.HasAnyFlag(DirtMarkType.BuildQuadTree))
                {
                    ManageQuadTree(!hadDirtyGraphics);
                    ClearDirtMarks(DirtMarkType.BuildQuadTree);
                }

                if (DirtMarks.HasAnyFlag(DirtMarkType.ShadowColor))
                    _shadowSprite.SetColor(_shadowColor);

                UpdateTotalBoundaries();
                SpecialBoundaryUpdate(_totalBoundaries, out _totalBoundaries);

                if (DirtMarks.HasAnyFlag(DirtMarkType.ShadowMath))
                    UpdateShadowSprite();
                _shadowAvailable = _useShadow && _shadowTex != null;

                ClearDirtMarks();
                InvokeMarkedDirty(DirtMarkType.Boundaries);
            }
            Dirty = false;
        }

        private void ManageQuadTree(bool buildTree)
        {
            if (_buildCharQuadTree)
            {
                if (buildTree)
                    BuildQuadTree();
            }
            else
                _charQuadTree.ClearPool();
        }

        private void GetLines()
        {
            if (_textGlyphs.Count == 0)
                return;

            if (_align != TextAlignment.Center && _align != TextAlignment.Right)
                return;

            float width = 0;
            void AddLine(int index)
            {
                _lines.Add(new LineInfo(width, index));
                width = 0;
            }

            for (int i = 0; i < _textGlyphs.Count; i++)
            {
                ref BitmapFont.Glyph glyph = ref _textGlyphs.GetReferenceAt(i);
                if (glyph.FontRegion != null)
                {
                    float right = glyph.Position.X + glyph.FontRegion.Width;
                    if (right > width)
                        width = right;
                }

                if (glyph.Character == '\n')
                    AddLine(i);
            }
            AddLine(_textGlyphs.Count - 1);

            float GetXOffset(float lineWidth)
            {
                switch (_align)
                {
                    case TextAlignment.Center:
                        return -lineWidth / 2;

                    case TextAlignment.Right:
                        return -lineWidth;

                    default:
                        return 0;
                }
            }

            int lastBreak = 0;
            int lineCount = _lines.Count;
            float largestOffset = 0;
            for (int i = 0; i < lineCount; i++)
            {
                int breakIndex = _lines[i].BreakIndex;
                int charCount = i == lineCount - 1 ? breakIndex + 1 : breakIndex;
                for (int j = lastBreak; j < charCount; j++)
                {
                    ref BitmapFont.Glyph glyph = ref _textGlyphs.GetReferenceAt(j);
                    float xOffset = GetXOffset(_lines[i].Width);

                    Vector2 newPos = glyph.Position;
                    newPos.X += xOffset;
                    glyph = new BitmapFont.Glyph(glyph.Character, newPos, glyph.FontRegion);

                    if (Math.Abs(xOffset) > Math.Abs(largestOffset))
                        largestOffset = xOffset;
                }
                lastBreak = breakIndex;
            }

            _alignOffsetX = largestOffset * Scale.X;
            _textBounds.X += _alignOffsetX;
            _lines.Clear(false);
        }

        public void ProcessTextIfNeeded()
        {
            lock (SyncRoot)
            {
                if (HasAnyDirtMark(NEEDS_PROCESSING_MARKS))
                    ProcessText(new SpecialTextFormat.Input(_value));
            }
        }

        internal void ProcessText(SpecialTextFormat.Input input)
        {
            _expressionColors.Clear(false);
            _processedText.Clear();

            SpecialBeforeTextProcessing();
            SpecialProcessedTextLength = _processedText.Length;
            if (input.Length > 0)
            {
                var colorList = _valueExpressions ? _expressionColors : null;
                SpecialTextFormat.Format(input, _processedText, _color, _font, _keepExpressions, colorList);
            }
            SpecialPostTextProcessing();

            _textGlyphs.Clear(false);
            _textBounds.Position = GlobalPosition.ToVector2();
            _textBounds.Size = _font.GetGlyphs(_processedText, _textGlyphs);
            GetLines();
            _textBounds.Size *= Scale;
            UpdateTotalBoundaries();

            ClearDirtMarks(NEEDS_PROCESSING_MARKS);
            InvokeMarkedDirty(DirtMarkType.Boundaries);
            MarkDirty(DirtMarkType.ValueProcessed, true);
        }

        protected virtual void SpecialBeforeTextProcessing()
        {
        }

        protected virtual void SpecialPostTextProcessing()
        {
        }

        protected virtual void SpecialBoundaryUpdate(RectangleF input, out RectangleF output)
        {
            output = input;
        }

        protected void PrepareQuadTree()
        {
            _charQuadTree.ClearTree();
            _charQuadTree.Resize(_textBounds);
        }

        protected void BuildQuadTree()
        {
            PrepareQuadTree();
            Vector3 globalPos = GlobalPosition;
            for (int i = 0, count = _textSprites.Count; i < count; i++)
                AddTextQuadItem(ref globalPos, ref _textSprites.GetReferenceAt(i));
        }

        protected void AddTextQuadItem(ref Vector3 pos, ref GlyphBatchedSprite item)
        {
            float yDiff = item.Sprite.TL.Position.Y - pos.Y;
            int line = (int)Math.Floor(yDiff / Scale.Y / _font.LineHeight);

            float x = (float)Math.Floor(item.Sprite.TL.Position.X);
            float y = (float)Math.Floor(pos.Y) + line * _font.LineHeight * Scale.Y;
            float w = (float)Math.Floor(item.Sprite.BR.Position.X - x) * 0.5f;
            float h = _font.LineHeight * Scale.Y * 0.5f;

            float o = 0.001f;
            var node = new RectangleF(x + o, y + h * 0.5f, w - o, h);

            _charQuadTree.CurrentTree.Insert(node, item.Index);

            node.X += w;
            _charQuadTree.CurrentTree.Insert(node, item.Index + 0.5f);
        }

        protected void UpdateTotalBoundaries()
        {
            _totalBoundaries = _textBounds + _shadowOffset;
        }

        protected void UpdateShadowSprite()
        {
            if (_shadowTexSrc.X <= 0 || _shadowTexSrc.Y <= 0)
                return;

            Matrix2 matrix = BatchedSprite.GetMatrixFromRect(
                _totalBoundaries, Origin, Rotation, _shadowTexSrc);

            _shadowSprite.SetTransform(matrix, _shadowTexSrc);
            _shadowSprite.SetDepth(Z);
            _shadowSprite.SetTexCoords(_shadowTex);
        }

        protected void SetAndProcessText(StringBuilder value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            lock (SyncRoot)
            {
                MarkDirty(DirtMarkType.Value, true);
                ProcessText(new SpecialTextFormat.Input(value));
            }
        }

        public override void Draw(GameTime time, SpriteBatch batch)
        {
            lock (SyncRoot)
            {
                BuildGraphicsIfNeeded();

                if (_textSprites.Count > 0)
                {
                    if (_shadowAvailable)
                        batch.Draw(_shadowTex.Texture, _shadowSprite);

                    batch.DrawString(_textSprites);
                }
            }
        }

        public struct LineInfo
        {
            public float Width;
            public int BreakIndex;

            public LineInfo(float width, int breakIndex)
            {
                Width = width;
                BreakIndex = breakIndex;
            }
        }
    }
}
