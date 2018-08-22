using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.TextureAtlases;
using System;

namespace GeneralShare.UI
{
    public class UIProgressBar : UIElement, IProgress<float>
    {
        private TextureRegion2D _mainBarRegion;
        private TextureRegion2D _backBarRegion;
        
        private RectangleF _boundaries;
        private BatchedSprite _backSprite;
        private BatchedSprite _mainSprite;

        private BarDirection _direction;
        private int _barThickness;
        private float _value;
        private Range<float> _range;
        private Vector2 _bounds;
        private Vector2 _headPos;
        private RectangleF _mainRect;
        private bool _needsSpriteUpdate;

        public override RectangleF Boundaries { get { UpdateBar(); return _boundaries; } }
        public int BackBarThickness { get => _barThickness; set => SetThickness(value); }
        public Vector2 Bounds { get => _bounds; set => SetBounds(value); }
        public RectangleF MainBarRect { get { UpdateBar(); return _mainRect; } }
        public Vector2 BarHeadPosition { get { UpdateBar(); return _headPos; } }
        public BarDirection Direction { get => _direction; set => SetDirection(value); }

        public Range<float> Range { get => _range; set => SetRange(value); }
        public float Value { get => _value; set => SetValue(value); }
        public float FillPercentage => Mathf.Map(_value, _range.Min, _range.Max, 0, 1);

        public Color MainColor { get => _mainSprite.TL.Color; set => SetMainColor(value); }
        public Color BackgroundColor { get => _backSprite.TL.Color; set => SetBackColor(value); }

        public UIProgressBar(
            UIManager manager, TextureRegion2D mainBarRegion, TextureRegion2D backBarRegion) :
            base(manager)
        {
            _mainBarRegion = mainBarRegion;
            _backBarRegion = backBarRegion;

            MainColor = Color.White;
            BackgroundColor = Color.White;
            BackBarThickness = 1;
            Range = new Range<float>(0, 1);
            _direction = BarDirection.ToRight;
        }

        public UIProgressBar(UIManager manager) :
            this(manager, manager.GrayscaleRegion, manager.WhitePixelRegion)
        {
        }
        
        public UIProgressBar(TextureRegion2D mainBarRegion, TextureRegion2D backBarRegion) :
            this(null, mainBarRegion, backBarRegion)
        {
        }

        private void SetMainColor(Color value)
        {
            _mainSprite.SetColor(value);
        }

        private void SetBackColor(Color value)
        {
            _backSprite.SetColor(value);
        }

        private void SetThickness(int value)
        {
            MarkDirtyE(ref _barThickness, value, DirtMarkType.BarThickness);
        }

        private void SetDirection(BarDirection value)
        {
            MarkDirty(ref _direction, value, DirtMarkType.BarDirection);
        }

        private void SetBounds(Vector2 value)
        {
            MarkDirtyE(ref _bounds, value, DirtMarkType.Bounds);
        }

        private void SetValue(float value)
        {
            MarkDirtyE(ref _value, MathHelper.Clamp(value, _range.Min, _range.Max), DirtMarkType.Value);
        }
        
        private void SetRange(Range<float> value)
        {
            MarkDirtyE(ref _range, value, DirtMarkType.Range);
        }

        public void Report(float value)
        {
            SetValue(value);
        }
        
        private void CalculateBackSprite()
        {
            var matrix = Matrix2.CreateFrom(Position.ToVector2(), Rotation, Boundaries.Size, Origin);
            _backSprite.SetTransform(matrix, _backBarRegion.Bounds.Size);
            _backSprite.SetDepth(Position.Z);
            _backSprite.SetTexCoords(_backBarRegion);
        }

        private void CalculateMainSprite(RectangleF mainDst)
        {
            mainDst.X += _barThickness;
            mainDst.Y += _barThickness;
            mainDst.Width -= _barThickness * 2;
            mainDst.Height -= _barThickness * 2;

            var size = new Vector2(Scale.X * mainDst.Width, Scale.Y * mainDst.Height);
            var matrix = Matrix2.CreateFrom(mainDst.Position, Rotation, size, Origin);
            
            _mainSprite.SetTransform(matrix, _mainBarRegion.Bounds.Size);
            _mainSprite.SetTexCoords(_mainBarRegion);
        }

        private void UpdateMainRect()
        {
            float w = _bounds.X * FillPercentage;
            float h = _bounds.Y / _mainBarRegion.Height;
            switch (_direction)
            {
                //TODO: Add more directions

                case BarDirection.ToRight:

                    _headPos = new Vector2(w, 0);
                    _mainRect = new RectangleF(Position.X, Position.Y, w, h);
                    break;

                case BarDirection.ToLeft:
                    float hPos = Position.X + _bounds.X - w;
                    _headPos = new Vector2(hPos, 0);
                    _mainRect = new RectangleF(hPos, Position.Y, w, h);
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        private void UpdateBar()
        {
            if (Dirty)
            {
                UpdateMainRect();

                _boundaries.X = Position.X;
                _boundaries.Y = Position.Y;
                _boundaries.Width = Scale.X * _bounds.X / _backBarRegion.Width;
                _boundaries.Height = Scale.Y * _bounds.Y / _backBarRegion.Height;
                InvokeMarkedDirty(DirtMarkType.Boundaries);

                _needsSpriteUpdate = true;

                ClearDirtMarks();
                Dirty = false;
            }
        }

        public override void Draw(GameTime time, SpriteBatch batch)
        {
            if (_needsSpriteUpdate)
            {
                CalculateMainSprite(_mainRect);
                if (DirtMarks != DirtMarkType.BarThickness)
                    CalculateBackSprite();

                _needsSpriteUpdate = false;
            }

            if (BackBarThickness >= 0)
                batch.Draw(_backBarRegion.Texture, _backSprite);
            batch.Draw(_mainBarRegion.Texture, _mainSprite);
        }

        public enum BarDirection
        {
            ToRight, ToLeft, ToTop, ToBottom
        }
    }
}
