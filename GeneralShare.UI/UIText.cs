using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.BitmapFonts;
using System.Text;

namespace GeneralShare.UI
{
    public class UIText : UIElement
    {
        private BitmapFont _font;
        private TextSegment _segment;
        private Vector2 _startingPoint;
        private RectangleF _boundaries;

        private TextAlignment _alignment;
        private Vector2 _alignmentOffset;

        public ICharIterator Text
        {
            set
            {
                using (var iterator = value)
                    SetText(iterator);
            }
        }

        public BitmapFont Font
        {
            get => _font;
            set => MarkDirty(ref _font, value, DirtMarkType.Font);
        }

        public override RectangleF Boundaries { get { AssertPure(); return _boundaries; } }
        public SizeF Measure { get { AssertPure(); return _segment.Measure; } }

        public Color Color { get => _segment.Color; set => SetColor(value); }
        public TextAlignment Alignment { get => _alignment; set => SetAlignment(value); }

        public bool IsShadowed = true;
        public Color ShadowColor = new Color(Color.Black, 0.75f);
        public RectangleF ShadowOffset = new RectangleF(-4, 10, 8, -16);

        public UIText(UIManager manager, BitmapFont font) : base(manager)
        {
            _font = font;
            _segment = new TextSegment();
        }

        private void AssertPure()
        {
            if (IsDirty)
                NeedsCleanup();
        }

        private void SetText(ICharIterator iterator)
        {
            _segment.Initialize(_font, iterator);
            MarkDirty(DirtMarkType.Value);
        }

        public void SetText(string value, int offset, int length)
        {
            using (var iter = value.ToIterator(offset, length))
                SetText(iter);
        }

        public void SetText(string value)
        {
            SetText(value, 0, value.Length);
        }

        public void SetText(StringBuilder value, int offset, int length)
        {
            using (var iter = value.ToIterator(offset, length))
                SetText(iter);
        }

        public void SetText(StringBuilder value)
        {
            SetText(value, 0, value.Length);
        }

        private void SetColor(Color value)
        {
            if(MarkDirty(Color, value, DirtMarkType.Color))
                _segment.Color = value;
        }

        private void SetAlignment(TextAlignment value)
        {
            MarkDirty(ref _alignment, value, DirtMarkType.TextAlignment);
        }

        protected override void NeedsCleanup()
        {
            if (MarkClean(DirtMarkType.Position))
            {
                UpdateBoundaries();
                return;
            }

            if (HasDirtMarks(DirtMarkType.TextAlignment))
            {
                CalculateAlignmentOffset();
                if (MarkClean(DirtMarkType.TextAlignment))
                    return;
            }

            _segment.Scale = GlobalScale;
            _segment.Rebuild();
            CalculateAlignmentOffset();

            if(HasDirtMarks(DirtMarkType.Shadowed | DirtMarkType.ShadowOffset | DirtMarkType.ShadowColor))
                UpdateShadow();

            UpdateBoundaries();

            MarkClean();
        }

        private void UpdateShadow()
        {
            if (IsShadowed)
            {

            }
        }

        private void UpdateBoundaries()
        {
            _startingPoint = GlobalPosition.ToVector2() + _alignmentOffset;
            _boundaries = new RectangleF(_startingPoint, _segment.Measure);
        }

        private void CalculateAlignmentOffset()
        {
            switch (Alignment)
            {
                default:
                //case TextAlignment.Left:
                    _alignmentOffset = Vector2.Zero;
                    break;

                case TextAlignment.Center:
                    _alignmentOffset = -new Vector2(_segment.Measure.Width, 0) / 2f;
                    break;

                case TextAlignment.Right:
                    _alignmentOffset = -new Vector2(_segment.Measure.Width, 0);
                    break;
            }
        }

        public override void Draw(GameTime time, SpriteBatch batch)
        {
            batch.DrawString(_segment, _startingPoint);
            //batch.DrawRectangle(Boundaries, Color.Red, 1);
        }
    }
}