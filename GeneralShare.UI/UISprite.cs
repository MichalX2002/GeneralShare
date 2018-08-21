using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.TextureAtlases;

namespace GeneralShare.UI
{
    public class UISprite : UIElement
    {
        private TextureRegion2D _texture;
        private BatchedSprite _sprite;
        private RectangleF _boundaries;

        public override RectangleF Boundaries => _boundaries;

        public TextureRegion2D Texture { get => _texture; set => SetTexture(value); }
        public Color Color { get => _sprite.TL.Color; set => _sprite.SetColor(value); }

        public UISprite(TextureRegion2D texture, UIManager manager) : base(manager)
        {
            Color = Color.White;
        }

        private void SetTexture(TextureRegion2D value)
        {
            MarkDirty(ref _texture, value, DirtMarkType.Value);
        }

        private void UpdateSprite()
        {
            if (DirtMarks.HasFlags(DirtMarkType.Transform, DirtMarkType.Value))
            {
                _boundaries = new RectangleF(X, Y, Texture.Width * Scale.X, Texture.Height * Scale.Y);
                InvokeMarkedDirty(DirtMarkType.Boundaries);

                var pos = _position.ToVector2();
                var srcSize = _texture.Bounds.Size.ToVector2();

                _sprite.SetTransform(pos, _rotation, _scale, _origin * srcSize, srcSize);
                _sprite.SetDepth(_position.Z);
                _sprite.SetTexCoords(_texture);
            }
            ClearDirtMarks();
        }

        public override void Draw(GameTime time, SpriteBatch batch)
        {
            lock (SyncRoot)
            {
                if (Dirty)
                {
                    UpdateSprite();
                    Dirty = false;
                }
            }
            batch.Draw(_texture.Texture, _sprite);
        }
    }
}
