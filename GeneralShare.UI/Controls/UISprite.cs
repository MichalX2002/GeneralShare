using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.TextureAtlases;

namespace GeneralShare.UI
{
    public class UISprite : UIElement
    {
        private TextureRegion2D _region;
        private BatchedSprite _sprite;
        private RectangleF _boundaries;
        private RectangleF _destination;

        public override RectangleF Boundaries { get { Purify(); return _boundaries; } }
        public TextureRegion2D Region { get => _region; set => SetTexture(value); }
        public Color Color { get => _sprite.TL.Color; set => _sprite.SetColor(value); }
        public RectangleF Destination { get => _destination; set => SetDestination(value); }
        public bool IsUsingDestination => Destination.Width > 0 && Destination.Height > 0;

        public UISprite(UIManager manager, TextureRegion2D texture) : base(manager)
        {
            Region = texture;
            Color = Color.White;
        }

        private void SetTexture(TextureRegion2D value)
        {
            MarkDirty(ref _region, value, DirtMarkType.Value);
        }

        private void SetDestination(RectangleF value)
        {
            MarkDirty(ref _destination, value, DirtMarkType.Destination);
        }

        protected override void Cleanup()
        {
            if (HasDirtMarks(DirtMarkType.Transform | DirtMarkType.Value | DirtMarkType.Destination))
            {
                SizeF srcSize = _region.Size;
                if (IsUsingDestination)
                {
                    var dst = _destination;
                    if (Anchor != null)
                    {
                        Anchor.Purify();
                        dst.Position += Anchor.Position.ToVector2();
                        dst = ApplyAnchorOffset(dst);
                    }

                    var matrix = BatchedSpriteExtensions.GetMatrixFromRect(dst, Origin, -Rotation, srcSize);
                    _sprite.SetTransform(matrix, srcSize);
                    _boundaries = dst;
                }
                else
                {
                    Vector2 pos = GlobalPosition.ToVector2();
                    Vector2 scale = GlobalScale;
                    _boundaries = new RectangleF(pos.X, pos.Y, Region.Width * scale.X, Region.Height * scale.Y);
                    _sprite.SetTransform(pos, -GlobalRotation, scale, Origin * srcSize, srcSize);
                }
                _sprite.SetDepth(Z);
                _sprite.SetTexCoords(_region);

                InvokeMarkedDirty(DirtMarkType.Boundaries);
            }
            MarkClean();
        }

        private RectangleF ApplyAnchorOffset(RectangleF rect)
        {
            switch (Anchor.Pivot)
            {
                case PivotPosition.Top:
                    rect.X -= rect.Width / 2f;
                    break;

                case PivotPosition.TopRight:
                    rect.X -= rect.Width;
                    break;

                case PivotPosition.Bottom:
                    rect.X -= rect.Width / 2f;
                    rect.Y += rect.Height;
                    break;

                case PivotPosition.BottomRight:
                    rect.X -= rect.Width;
                    rect.Y += rect.Height;
                    break;
            }

            return rect;
        }

        public override void Draw(GameTime time, SpriteBatch batch)
        {
            batch.Draw(_region.Texture, _sprite);
        }
    }
}
