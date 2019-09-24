using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace GeneralShare.UI
{
    public static class ThicknessHelper
    {
        public static RectangleF ToOffsetRectangle(this ThicknessF thickness, Vector2 scale)
        {
            return new RectangleF(
                ToOffsetPosition(thickness, scale),
                ToOffsetSize(thickness, scale));
        }

        public static PointF ToOffsetPosition(this ThicknessF thickness, Vector2 scale)
        {
            return new PointF(
                -thickness.Left * scale.X,
                -thickness.Top * scale.Y);
        }

        public static SizeF ToOffsetSize(this ThicknessF thickness, Vector2 scale)
        {
            return new SizeF(
                (thickness.Left + thickness.Right) * scale.X,
                (thickness.Top + thickness.Bottom) * scale.Y);
        }
    }
}
