using Microsoft.Xna.Framework;
using MonoGame.Extended.BitmapFonts;

namespace GeneralShare
{
    public static class ColorExtensions
    {
        public static string ToHex(this Color color)
        {
            var builder = StringBuilderPool.Rent(8);
            builder.AppendFormat("{0:x2}", color.R);
            builder.AppendFormat("{0:x2}", color.G);
            builder.AppendFormat("{0:x2}", color.B);
            builder.AppendFormat("{0:x2}", color.A);

            string result = builder.ToString();
            StringBuilderPool.Return(builder);
            return result;
        }
    }
}
