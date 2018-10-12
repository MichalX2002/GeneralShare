using Microsoft.Xna.Framework;

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
    }
}
