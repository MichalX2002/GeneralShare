using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace GeneralShare.UI
{
    public static class ThicknessHelper
    {
        public static RectangleF ToOffsetRectangle(this ThicknessF thickness, Vector2 scale)
        {
            return new RectangleF(
                -thickness.Left * scale.X,
                -thickness.Top * scale.Y,
                (thickness.Left + thickness.Right) * scale.X,
                (thickness.Top + thickness.Bottom) * scale.Y);
        }
    }
}
