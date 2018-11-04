using System;
using System.IO;

namespace GeneralShare
{
    public static class FileExtensions
    {
        private static readonly string[] SIZE_SUFFIXES = {
            "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB"
        };

        public static string GetFullNameWithoutExtension(this FileInfo file)
        {
            int extLength = file.Extension.Length;
            string fullName = file.FullName;

            return fullName.Substring(0, fullName.Length - extLength);
        }

        public static string GetNameWithoutExtension(this FileInfo file)
        {
            int extLength = file.Extension.Length;
            string name = file.Name;

            return name.Substring(0, name.Length - extLength);
        }

        public static double ToReadableLength(this FileInfo file, int decimals, out string suffix)
        {
            return ToReadableLength(file.Length, decimals, out suffix);
        }

        public static double ToReadableLength(this long value, int decimals, out string suffix)
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
            if (Math.Round(adjustedSize, decimals) >= 1000)
            {
                mag += 1;
                adjustedSize /= 1024;
            }

            suffix = SIZE_SUFFIXES[mag];
            return (double)Math.Round(adjustedSize, decimals);
        }
    }
}
