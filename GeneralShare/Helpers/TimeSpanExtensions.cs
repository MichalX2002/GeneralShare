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
                    timeSpan.TotalSeconds < 0.01 ?
                    (int)(timeSpan.TotalMilliseconds * 100f) / 100f : timeSpan.Milliseconds;

                return $"{millis}ms";
            }
            else
            {
                float seconds =
                    timeSpan.TotalSeconds < 1 ?
                    (int)(timeSpan.TotalSeconds * 100) / 100f :
                    timeSpan.TotalSeconds < 10 ?
                    (int)(timeSpan.TotalSeconds * 10) / 10f :
                    timeSpan.Seconds;
                return $"{seconds}s";
            }
        }

        /// <summary>
        /// Hours : Minutes : Seconds
        /// </summary>
        public static string ToHMS(this TimeSpan timeSpan)
        {
            string h = timeSpan.Hours.ToString("0");
            string m = timeSpan.Minutes.ToString("00");
            string s = timeSpan.Seconds.ToString("00");
            return $"{h}:{m}:{s}";
        }

        /// <summary>
        /// Hours : Minutes : Seconds . Fraction
        /// </summary>
        public static string ToHMSF(this TimeSpan timeSpan)
        {
            return $"{ToHMS(timeSpan)}.{(int)(timeSpan.Milliseconds * 0.01)}";
        }
    }
}
