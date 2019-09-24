using System;
using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace GeneralShare
{
    public static class Mathf
    {
        public static float Map(float value, float srcMin, float srcMax, float dstMin, float dstMax)
        {
            return (value - srcMin) / (srcMax - srcMin) * (dstMax - dstMin) + dstMin;
        }
     
        public static float Lerp(float src, float dst, float amount)
        {
            return MathHelper.Lerp(src, dst, amount);
        }

        public static int Clamp(int value, int min, int max)
        {
            if (value < min)
                return min;
            if (value > max)
                return max;
            return value;
        }

        public static float Clamp(float value, float min, float max)
        {
            if (value < min)
                return min;
            if (value > max)
                return max;
            return value;
        }

        public static double Clamp(double value, double min, double max)
        {
            if (value < min)
                return min;
            if (value > max)
                return max;
            return value;
        }

        public static SizeF Clamp(SizeF value, SizeF min, SizeF max)
        {
            value.Width = Clamp(value.Width, min.Width, max.Width);
            value.Height = Clamp(value.Height, min.Height, max.Height);
            return value;
        }

        public static float Round(float value)
        {
            return (float)Math.Round(value);
        }

        public static float Round(float value, int decimals)
        {
            return (float)Math.Round(value, decimals);
        }

        public static float Round(float value, int decimals, MidpointRounding mode)
        {
            return (float)Math.Round(value, decimals, mode);
        }
    }
}
