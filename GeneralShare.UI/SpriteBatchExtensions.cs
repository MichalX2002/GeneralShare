
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;

namespace GeneralShare.UI
{
    public static class SpriteBatchExtensions
    {
        public static void DrawString(this SpriteBatch batch, TextSegment segment, Vector2 position)
        {
            batch.DrawString(segment._spriteList, position);
        }
    }
}
