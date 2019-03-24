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
        private Vector2 _destination;
        private Vector2 _headPos;
        private RectangleF _mainRect;

        public override RectangleF Boundaries { get { Purify(); return _boundaries; } }
        public int BackBarThickness { get => _barThickness; set => SetThickness(value); }
        public Vector2 Destination { get => _destination; set => SetDestination(value); }
        public RectangleF MainBarRect { get { Purify(); return _mainRect; } }
        public Vector2 BarHeadPosition { get { Purify(); return _headPos; } }
        public BarDirection Direction { get => _direction; set => SetDirection(value); }

        public TextureRegion2D MainBarRegion { get => _mainBarRegion; set => SetMainRegion(value); }
        public TextureRegion2D BackBarRegion { get => _backBarRegion; set => SetBackRegion(value); }

        public Range<float> Range { get => _range; set => SetRange(value); }
        public float Value { get => _value; set => SetValue(value); }
        public float FillPercentage => Mathf.Map(_value, _range.Min, _range.Max, 0, 1);

        public Color MainColor { get => _mainSprite.TL.Color; set => SetMainColor(ref value); }
        public Color BackgroundColor { get => _backSprite.TL.Color; set => SetBackColor(ref value); }

        public UIProgressBar(
            UIManager manager, TextureRegion2D mainBarRegion, TextureRegion2D backBarRegion) : base(manager)
        {
            _mainBarRegion = mainBarRegion;
            _backBarRegion = backBarRegion;

            MainColor = Color.White;
            BackgroundColor = Color.Gray;
            BackBarThickness = 1;
            Range = new Range<float>(0, 1);
            _direction = BarDirection.ToRight;
        }

        public UIProgressBar(UIManager manager) :
            this(manager, manager.WhitePixelRegion, manager.WhitePixelRegion)
        {
        }

        private void SetMainRegion(TextureRegion2D value)
        {
            MarkDirty(ref _mainBarRegion, value, DirtMarkType.Texture);
        }

        private void SetBackRegion(TextureRegion2D value)
        {
            MarkDirty(ref _backBarRegion, value, DirtMarkType.Texture);
        }

        private void SetMainColor(ref Color value)
        {
            _mainSprite.SetColor(value);
        }

        private void SetBackColor(ref Color value)
        {
            _backSprite.SetColor(value);
        }

        private void SetThickness(int value)
        {
            MarkDirty(ref _barThickness, value, DirtMarkType.BarThickness);
        }

        private void SetDirection(BarDirection value)
        {
            MarkDirty(ref _direction, value, DirtMarkType.BarDirection);
        }

        private void SetDestination(Vector2 value)
        {
            MarkDirty(ref _destination, value, DirtMarkType.Destination);
        }

        private void SetValue(float value)
        {
            MarkDirty(ref _value, MathHelper.Clamp(value, _range.Min, _range.Max), DirtMarkType.Value);
        }
        
        private void SetRange(Range<float> value)
        {
            MarkDirty(ref _range, value, DirtMarkType.Range);
        }

        public void Report(float value)
        {
            SetValue(value);
        }
        
        private void CalculateBackSprite()
        {
            var matrix = Matrix2.CreateFrom(GlobalPosition.ToVector2(), Rotation, _boundaries.Size, Origin);
            _backSprite.SetTransform(matrix, _backBarRegion.Bounds.Size);
            _backSprite.SetDepth(Z);
            _backSprite.SetTexCoords(_backBarRegion);
        }

        private void CalculateMainSprite(RectangleF mainDst)
        {
            mainDst.X += _barThickness;
            mainDst.Y += _barThickness;

            Vector2 scale = GlobalScale;
            mainDst.Width *= scale.X;
            mainDst.Height *= scale.Y;

            mainDst.Width -= _barThickness * 2 / _mainBarRegion.Width;
            mainDst.Height -= _barThickness * 2 / _mainBarRegion.Height;

            var matrix = Matrix2.CreateFrom(mainDst.Position, Rotation, mainDst.Size, Origin);

            _mainSprite.SetTransform(matrix, _mainBarRegion.Bounds.Size);
            _mainSprite.SetDepth(Z);
            _mainSprite.SetTexCoords(_mainBarRegion);
        }

        private void UpdateMainRect()
        {
            float w = _destination.X * FillPercentage;
            float h = _destination.Y / _mainBarRegion.Height;
            Vector3 pos = GlobalPosition;

            switch (_direction)
            {
                //TODO: Add more directions

                case BarDirection.ToRight:
                    _headPos = new Vector2(w, 0);
                    _mainRect = new RectangleF(pos.X, pos.Y, w, h);
                    break;

                case BarDirection.ToLeft:
                    float headX = pos.X + _destination.X - w;
                    _headPos = new Vector2(headX, 0);
                    _mainRect = new RectangleF(headX, pos.Y, w, h);
                    break;

                default:
                    throw new NotImplementedException(_direction.ToString());
            }
        }

        protected override void Cleanup()
        {
            UpdateMainRect();

            Vector3 pos = GlobalPosition;
            _boundaries.X = pos.X;
            _boundaries.Y = pos.Y;

            Vector2 scale = GlobalScale;
            _boundaries.Width = scale.X * _destination.X / _backBarRegion.Width;
            _boundaries.Height = scale.Y * _destination.Y / _backBarRegion.Height;
            InvokeMarkedDirty(DirtMarkType.Boundaries);

            CalculateMainSprite(_mainRect);
            if (DirtMarks != DirtMarkType.BarThickness)
                CalculateBackSprite();
            MarkClean();
        }

        public override void Draw(GameTime time, SpriteBatch batch)
        {
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
