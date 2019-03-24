using System;

namespace GeneralShare
{
    public static class FileSizeHelper
    {
        private static readonly string[] SIZE_SUFFIXES = {
            "", "kB", "MB", "GB", "TB", "PB", "EB"
        };

        public static double ToReadableLength(long value, int decimals, out string suffix)
        {
            if (decimals < 0)
                throw new ArgumentOutOfRangeException(nameof(decimals));

            if (value < 0 || value == 0)
            {
                suffix = SIZE_SUFFIXES[0];
                return value;
            }

            // mag is 0 for bytes, 1 for KB, 2, for MB, etc.
            int mag = (int)Math.Log(value, 1024);

            // 1L << (mag * 10) == 2 ^ (10 * mag) 
            // [i.e. the number of bytes in the unit corresponding to mag]
            decimal adjustedSize = (decimal)value / (1L << (mag * 10));

            // make adjustment when the value is large enough that
            // it would round up to 1000 or more
            //if (Math.Round(adjustedSize, decimals) >= 1000)
            //{
            //    mag += 1;
            //    adjustedSize /= 1024;
            //}

            suffix = SIZE_SUFFIXES[mag];
            return (double)Math.Round(adjustedSize, decimals);
        }

        public static string ToReadableSize(long value, int decimals = -1)
        {
            if(decimals == -1)
            {
                decimals = 0;
                if (value >= 1024 * 1024 * 1024)
                {
                    // 1gb
                    decimals = 2;
                }
                else if (value >= 1024 * 1024 * 100)
                {
                    // 100mb
                    decimals = 0;
                }
                else if (value >= 1024 * 1024 * 10)
                {
                    // 10mb
                    decimals = 1;
                }
                else if (value >= 1024 * 1024)
                {
                    // 1mb
                    decimals = 2;
                }
            }
            if (value < 1024)
                decimals = 0;

            double length = ToReadableLength(value, decimals, out string suffix);
            string format = GetFormat(decimals);
            return length.ToString(format) + suffix;
        }

        private static string GetFormat(int decimals)
        {
            switch (decimals)
            {
                case 1: return "0.0";
                case 2: return "0.00";
                case 3: return "0.000";
                case 4: return "0.0000";
                case 5: return "0.00000";

                default:
                    if (decimals <= 0)
                        return "0";
                    return "0." + new string('0', decimals);
            }
        }
    }
}
