using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GeneralShare.UI
{
    public static class DrawExtensions
    {
        public static void Draw(this SpriteBatch batch, GameTime time, UIElement component)
        {
            component.Draw(time, batch);
        }
    }
}
