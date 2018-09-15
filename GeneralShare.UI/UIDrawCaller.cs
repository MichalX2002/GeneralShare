
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GeneralShare.UI
{
    public class UIDrawCaller : UITransform
    {
        public delegate void DrawDelegate(
            GameTime time, SpriteBatch batch,
            Vector3 position, Vector2 origin, Vector2 scale, float rotation);

        public event DrawDelegate OnDraw;

        public UIDrawCaller()
        {
        }

        public UIDrawCaller(UIManager manager) : base(manager)
        {
        }

        public override void Draw(GameTime time, SpriteBatch batch)
        {
            OnDraw.Invoke(time, batch, GlobalPosition, Origin, GlobalScale, GlobalRotation);
        }
    }
}
