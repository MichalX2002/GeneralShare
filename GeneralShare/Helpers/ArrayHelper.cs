using System;
using System.Text;

namespace GeneralShare
{
    public static class ArrayHelper
    {
        [ThreadStatic]
        private static StringBuilder __stringBuilder;

        private static StringBuilder StringBuilder
        {
            get
            {
                if (__stringBuilder == null)
                    __stringBuilder = new StringBuilder();

                return __stringBuilder;
            }
        }

        public static bool Equal(this byte[] source, byte[] value)
        {
            if (source.Length != value.Length)
                return false;

            for (int i = 0; i < source.Length; i++)
            {
                if (source[i] != value[i])
                    return false;
            }
            return true;
        }
        
        public static bool StartsWith(this byte[] source, byte[] value)
        {
            if (source.Length < value.Length)
                return false;

            for (int i = 0; i < value.Length; i++)
            {
                if (source[i] != value[i])
                    return false;
            }

            return true;
        }
        
        public unsafe static bool StartsWith(byte* source, int srcLength, byte[] value)
        {
            if (srcLength < value.Length)
                return false;

            for (int i = 0; i < value.Length; i++)
            {
                if (source[i] != value[i])
                    return false;
            }

            return true;
        }

        public static string ToHex(this byte[] data)
        {
            var builder = StringBuilder.Clear();

            for (int i = 0; i < data.Length; i++)
                builder.Append(data[i].ToString("x2"));

            return builder.ToString();
        }

        public static string ToBase64(this byte[] data)
        {
            return Convert.ToBase64String(data);
        }
    }
}
