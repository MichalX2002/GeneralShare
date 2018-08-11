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
    public abstract class TextBoxBase : UIElement
    {
        protected BitmapFont _font;
        protected ListArray<CharDrawSprite> _textCache;
        protected ListArray<Color> _expressionColors;
        protected string _value;
        protected StringBuilder _processedText;
        protected bool _buildTextTree;
        protected PooledQuadTree<float> _textQuadTree;

        protected bool _valueExpressions;
        protected bool _keepExpressions;
        protected Color _baseColor;
        protected SizeF _measure;
        protected SizeF _scaledMeasure;
        protected Rectangle? _clipRect;
        protected RectangleF _textBounds;
        protected RectangleF _totalBoundaries;

        protected bool _useShadow;
        protected BatchedSprite _shadowSprite;
        protected TextureRegion2D _shadowTex;
        protected RectangleF _shadowOffset;
        protected Point _shadowTexSrc;
        protected Color _shadowColor;
        protected bool _shadowAvailable;

        public BitmapFont Font { get => _font; set => SetFont(value); }
        public StringBuilder ProcessedText { get { ProcessTextIfNeeded(); return _processedText; } }

        public Color BaseColor { get => _baseColor; set => SetColor(value); }
        public Rectangle? ClippingRectangle { get => _clipRect; set => SetClipRect(value); }

        public override RectangleF Boundaries { get { ProcessTextIfNeeded(); return _totalBoundaries; } }
        public RectangleF TextBoundaries { get { ProcessTextIfNeeded(); return _textBounds; } }
        public SizeF Measure { get { ProcessTextIfNeeded(); return _scaledMeasure; } }
        public TextureRegion2D ShadowTexture { get => _shadowTex; set => SetShadowTex(value); }
        public Color ShadowColor { get => _shadowColor; set => SetShadowColor(value); }
        public bool UseShadow { get => _useShadow; set => SetUseShadow(value); }
        public RectangleF ShadowOffset { get => _shadowOffset; set => SetShadowOffset(value); }
        public bool BuildCharQuadTree { get => _buildTextTree; set => SetBuildTextTree(value); }
        public ReadOnlyQuadTree<float> CharQuadTree => _textQuadTree.CurrentTree.AsReadOnly();
        public int SpecialProcessedTextLength { get; private set; }

        public TextBoxBase(UIContainer container, BitmapFont font) : base(container)
        {
            _value = string.Empty;
            _textCache = new ListArray<CharDrawSprite>();
            _expressionColors = new ListArray<Color>();
            _processedText = new StringBuilder();
            _textQuadTree = new PooledQuadTree<float>(Rectangle.Empty, 2, true);

            Font = font;
            BaseColor = Color.White;
            Scale = Vector2.One;

            SetShadowRadius(5, 0);
            ShadowColor = new Color(Color.Gray, 0.5f);
            if (container != null)
                ShadowTexture = container.WhitePixelRegion;
        }

        public TextBoxBase(BitmapFont font) : this(null, font) { }

        protected void SetBuildTextTree(bool value)
        {
            MarkDirtyE(ref _buildTextTree, value, DirtMarkType.BuildTextTree);
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

        protected void SetShadowColor(in Color value)
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

        protected void SetColor(in Color value)
        {
            MarkDirtyE(ref _baseColor, value, DirtMarkType.Color);
        }

        protected void SetClipRect(Rectangle? value)
        {
            MarkDirty(ref _clipRect, value, DirtMarkType.ClipRectangle);
        }

        public void BuildGraphicsIfNeeded()
        {
            if (Dirty == true)
            {
                BuildGraphics();
                ClearDirtMarks();
                Dirty = false;
            }
        }

        private void BuildGraphics()
        {
            ProcessTextIfNeeded();

            lock (_syncRoot)
            {
                if (HasDirtMarks(DirtMarkType.BuildTextTree))
                {
                    if (_buildTextTree == false)
                        _textQuadTree.ClearPool();

                    ClearDirtMarks(DirtMarkType.BuildTextTree);
                }

                DirtMarkType dirtyGraphics =
                    DirtMarkType.ValueProcessed | DirtMarkType.Transform |
                    DirtMarkType.Font | DirtMarkType.ClipRectangle;

                if (HasDirtMarks(dirtyGraphics))
                {
                    if (_processedText.Length > 0 && _font != null)
                    {
                        _textCache.Clear();
                        if (_textCache.Capacity < _processedText.Length)
                            _textCache.Capacity = _processedText.Length;

                        _font.GetGlyphSprites(_textCache, _processedText, _position.ToVector2(),
                            _baseColor, _rotation, _origin, _scale, _position.Z, _clipRect);

                        _textBounds.X = (int)_position.X;
                        _textBounds.Y = (int)_position.Y;

                        if (_buildTextTree)
                            BuildTextTree();

                        UpdateScaledMeasure();

                        if(_valueExpressions)
                            ApplyExpressionColors();

                        MarkDirty(DirtMarkType.ShadowMath, true);
                        HasContent = _textCache.Count > 0;
                    }
                    else
                        HasContent = false;
                }

                if (HasDirtMarks(DirtMarkType.ShadowColor))
                    _shadowSprite.SetColor(_shadowColor);

                UpdateTotalBoundaries();
                SpecialBoundaryUpdate(_totalBoundaries, out _totalBoundaries);

                if (HasDirtMarks(DirtMarkType.ShadowMath))
                    UpdateShadow();
                _shadowAvailable = _useShadow && _shadowTex != null;
            }
        }

        public void ProcessTextIfNeeded()
        {
            const DirtMarkType needsProcess =
                DirtMarkType.Color | DirtMarkType.Value | DirtMarkType.UseTextExpressions |
                DirtMarkType.Font | DirtMarkType.InputPrefix | DirtMarkType.KeepTextExpressions;

            lock (_syncRoot)
            {
                if (HasDirtMarks(needsProcess))
                {
                    _expressionColors.Clear();
                    _processedText.Clear();

                    SpecialBeforeTextProcessing();
                    if (_value != null)
                    {
                        var colorOutput = _valueExpressions ? _expressionColors : null;
                        SpecialTextFormat.Format(_value, _processedText,
                            _baseColor, _font, _keepExpressions, colorOutput);
                    }

                    SpecialPostTextProcessing();
                    SpecialProcessedTextLength = _processedText.Length;

                    _measure = _font.MeasureString(_processedText);
                    UpdateScaledMeasure();

                    _textBounds.Width = _scaledMeasure.Width;
                    _textBounds.Height = _scaledMeasure.Height;
                    UpdateTotalBoundaries();

                    ClearDirtMarks(needsProcess);
                    MarkDirty(DirtMarkType.ValueProcessed, true);
                }
            }
        }

        protected virtual void SpecialBeforeTextProcessing()
        {
        }

        protected virtual void SpecialPostTextProcessing()
        {
        }

        protected virtual void SpecialBoundaryUpdate(in RectangleF input, out RectangleF output)
        {
            output = input;
        }

        protected void BuildTextTree()
        {
            _textQuadTree.CurrentTree.Clear();
            _textQuadTree.Resize(_textBounds);

            for (int i = 0, count = _textCache.Count; i < count; i++)
            {
                ref CharDrawSprite item = ref _textCache.GetReferenceAt(i);
                ref BatchedSprite sprite = ref item.Sprite;

                float yDiff = sprite.TL.Position.Y - _position.Y;
                int line = (int)Math.Floor(yDiff / _scale.Y / _font.LineHeight);

                float x = (float)Math.Floor(sprite.TL.Position.X);
                float y = (float)Math.Floor(_position.Y) + line * _font.LineHeight * _scale.Y;
                float w = (float)Math.Floor(sprite.BR.Position.X - x) * 0.5f;
                float h = _font.LineHeight * _scale.Y * 0.5f;

                float o = 0.001f;
                var node = new RectangleF(x + o, y + h * 0.5f, w - o, h);

                _textQuadTree.CurrentTree.Insert(node, item.Index);

                node.X += w;
                _textQuadTree.CurrentTree.Insert(node, item.Index + 0.5f);
            }
        }

        protected void UpdateTotalBoundaries()
        {
            _totalBoundaries = _textBounds + _shadowOffset;
        }

        protected void UpdateScaledMeasure()
        {
            _scaledMeasure = _measure * _scale;
        }

        protected void UpdateShadow()
        {
            if (_shadowTexSrc.X <= 0 || _shadowTexSrc.Y <= 0)
                return;

            Matrix2 matrix = BatchedSprite.GetMatrixFromRect(
                _totalBoundaries, _origin, _rotation, _shadowTexSrc);

            _shadowSprite.SetTransform(matrix, _shadowTexSrc);
            _shadowSprite.SetDepth(_position.Z);
            _shadowSprite.SetTexCoords(_shadowTex);
        }

        protected void ApplyExpressionColors()
        {
            int expressionCount = _expressionColors.Count;
            if (expressionCount > 0)
            {
                int count = _textCache.Count;
                if (count > expressionCount)
                    count = expressionCount;

                for (int i = 0; i < count; i++)
                {
                    ref var item = ref _textCache.GetReferenceAt(i);
                    ref var color = ref _expressionColors.GetReferenceAt(i);
                    item.SetColor(color);
                }
            }
        }

        public override void Draw(GameTime time, SpriteBatch batch)
        {
            lock (_syncRoot)
            {
                BuildGraphicsIfNeeded();

                if (HasContent)
                {
                    if (_shadowAvailable)
                        batch.Draw(_shadowTex.Texture, _shadowSprite);

                    batch.DrawString(_textCache);
                }
            }
        }
    }
}
