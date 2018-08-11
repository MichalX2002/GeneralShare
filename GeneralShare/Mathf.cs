
namespace GeneralShare
{
    public static class Mathf
    {
        public static float Map(float value, float srcMin, float srcMax, float dstMin, float dstMax)
        {
            return (value - srcMin) / (srcMax - srcMin) * (dstMax - dstMin) + dstMin;
        }

    }
}
