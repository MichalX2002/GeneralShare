using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.TextureAtlases;
using System;

namespace GeneralShare.UI
{
    public class ProgressBar : UIElement, IProgress<float>
    {
        private TextureRegion2D _mainBarRegion;
        private TextureRegion2D _backBarRegion;
        
        private RectangleF _size;
        private BatchedSprite _backSprite;
        private BatchedSprite _mainSprite;

        private BarDirection _direction;
        private int _barThickness;
        private float _value;
        private Range<float> _range;
        private Vector2 _bounds;
        private Vector2 _headPos;

        public int BackBarThickness { get => _barThickness; set => SetThickness(value); }
        public Vector2 Bounds { get => _bounds; set => SetBounds(value); }
        public override RectangleF Boundaries { get { SetBoundaries(); return _size; } }
        public Vector2 BarHeadPosition { get { GetMainRect(); return _headPos; } }
        public BarDirection Direction { get => _direction; set => SetDirection(value); }

        public Range<float> Range { get => _range; set => SetRange(value); }
        public float Value { get => _value; set => SetValue(value); }
        public float FillPercentage => Mathf.Map(_value, _range.Min, _range.Max, 0, 1);

        public Color MainColor { get => _mainSprite.TL.Color; set => SetMainColor(value); }
        public Color BackgroundColor { get => _backSprite.TL.Color; set => SetBackColor(value); }

        public ProgressBar(UIManager manager, TextureRegion2D mainBarRegion,
            TextureRegion2D backBarRegion) : base(manager)
        {
            _mainBarRegion = mainBarRegion;
            _backBarRegion = backBarRegion;

            MainColor = Color.White;
            BackgroundColor = Color.White;
            BackBarThickness = 1;
            Range = new Range<float>(0, 1);
            _direction = BarDirection.ToRight;
        }

        public ProgressBar(UIManager manager) : this(manager, manager.GrayscaleRegion, manager.WhitePixelRegion)
        {
        }
        
        public ProgressBar(TextureRegion2D mainBarRegion, TextureRegion2D backBarRegion) :
            this(null, mainBarRegion, backBarRegion)
        {

        }

        private void SetMainColor(in Color value)
        {
            _mainSprite.SetColor(value);
        }

        private void SetBackColor(in Color value)
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

        private void SetBounds(in Vector2 value)
        {
            MarkDirtyE(ref _bounds, value, DirtMarkType.Bounds);
        }

        private void SetValue(float value)
        {
            MarkDirtyE(ref _value, MathHelper.Clamp(value, _range.Min, _range.Max), DirtMarkType.Value);
        }
        
        private void SetRange(in Range<float> value)
        {
            MarkDirtyE(ref _range, value, DirtMarkType.Range);
        }

        public void CalculateRectangles()
        {
            CalculateMainSprite();

            if (DirtMarks != DirtMarkType.BarThickness)
                CalculateBackSprite();

            ClearDirtMarks();
        }

        public void Report(float value)
        {
            SetValue(value);
        }

        private void SetBoundaries()
        {
            _size.X = _position.X;
            _size.Y = _position.Y;
            _size.Width = _scale.X * _bounds.X / _backBarRegion.Width;
            _size.Height = _scale.Y * _bounds.Y / _backBarRegion.Height;
        }

        private void CalculateBackSprite()
        {
            var matrix = Matrix2.CreateFrom(_position.ToVector2(), _rotation, Boundaries.Size, _origin);
            _backSprite.SetTransform(matrix, _backBarRegion.Bounds.Size);
            _backSprite.SetDepth(_position.Z);
            _backSprite.SetTexCoords(_backBarRegion);
        }

        private void CalculateMainSprite()
        {
            var mainDst = GetMainRect();
            mainDst.X += _barThickness;
            mainDst.Y += _barThickness;
            mainDst.Width -= _barThickness * 2;
            mainDst.Height -= _barThickness * 2;

            var size = new Vector2(_scale.X * mainDst.Width, _scale.Y * mainDst.Height);
            var matrix = Matrix2.CreateFrom(mainDst.Position, _rotation, size, _origin);
            
            _mainSprite.SetTransform(matrix, _mainBarRegion.Bounds.Size);
            _mainSprite.SetDepth(_position.Z);
            _mainSprite.SetTexCoords(_mainBarRegion);
        }

        private RectangleF GetMainRect()
        {
            float w = _bounds.X * FillPercentage;
            float h = _bounds.Y / _mainBarRegion.Height;
            switch (_direction)
            {
                //TODO: Add more directions

                case BarDirection.ToRight:
                    {
                        _headPos = new Vector2(w, 0);
                        return new RectangleF(_position.X, _position.Y, w, h);
                    }

                case BarDirection.ToLeft:
                    {
                        float hPos = _position.X + _bounds.X - w;
                        _headPos = new Vector2(hPos, 0);
                        return new RectangleF(hPos, _position.Y, w, h);
                    }

                default:
                    throw new NotImplementedException();
            }
        }

        public override void Draw(GameTime time, SpriteBatch batch)
        {
            if (Dirty)
            {
                CalculateRectangles();
                Dirty = false;
            }

            if (BackBarThickness >= 0)
                batch.Draw(_backBarRegion.Texture, _backSprite, _position.Z);
            batch.Draw(_mainBarRegion.Texture, _mainSprite, _position.Z);
        }

        public enum BarDirection
        {
            ToRight, ToLeft, ToTop, ToBottom
        }
    }
}
