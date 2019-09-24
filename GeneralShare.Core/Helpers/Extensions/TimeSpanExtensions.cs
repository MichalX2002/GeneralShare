using System;

namespace GeneralShare
{
    public static class TimeSpanExtensions
    {
        public static string ToPreciseString(this TimeSpan timeSpan)
        {
            if (timeSpan.TotalSeconds < 0.1)
            {
                float millis =
                    timeSpan.TotalSeconds < 0.01 ? (int)(timeSpan.TotalMilliseconds * 100f) / 100f : timeSpan.Milliseconds;

                return $"{millis}ms";
            }
            else
            {
                float seconds =
                    timeSpan.TotalSeconds < 1 ? (int)(timeSpan.TotalSeconds * 100) / 100f :
                    timeSpan.TotalSeconds < 10 ? (int)(timeSpan.TotalSeconds * 10) / 10f :
                    timeSpan.Seconds;
                return $"{seconds}s";
            }
        }

        /// <summary>
        /// Hours : Minutes : Seconds
        /// </summary>
        public static string ToHMS(this TimeSpan timeSpan, bool useSuffix = false)
        {
            string h = ((int)timeSpan.TotalHours).ToString("00");
            string m = timeSpan.Minutes.ToString("00");
            string s = timeSpan.Seconds.ToString("00");

            if (useSuffix)
                return $"{h}h:{m}m:{s}s";
            return string.Join(":", h, m, s);
        }

        /// <summary>
        /// Hours : Minutes
        /// </summary>
        public static string ToHM(this TimeSpan timeSpan, bool useSuffix = false)
        {
            string h = ((int)timeSpan.TotalHours).ToString("00");
            string m = timeSpan.Minutes.ToString("00");

            if (useSuffix)
                return $"{h}h:{m}m";
            return string.Join(":", h, m);
        }

        /// <summary>
        /// Hours : Minutes : Seconds . Fraction
        /// </summary>
        public static string ToHMSF(this TimeSpan timeSpan)
        {
            return string.Concat(ToHMS(timeSpan), ".", ((int)(timeSpan.Milliseconds * 0.01)).ToString());
        }
    }
}
