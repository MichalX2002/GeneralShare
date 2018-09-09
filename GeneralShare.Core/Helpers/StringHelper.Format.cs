using System.Text;

namespace GeneralShare
{
    public static partial class StringHelper
    {
        public static string Format(this string value, object arg0)
        {
            try
            {
                return string.Format(value, arg0);
            }
            catch
            {
                return FormatError(value, arg0);
            }
        }

        public static string Format(this string value, object arg0, object arg1)
        {
            try
            {
                return string.Format(value, arg0, arg1);
            }
            catch
            {
                return FormatError(value, arg0, arg1);
            }
        }

        public static string Format(this string value, object arg0, object arg1, object arg2)
        {
            try
            {
                return string.Format(value, arg0, arg1, arg2);
            }
            catch
            {
                return FormatError(value, arg0, arg1, arg2);
            }
        }

        public static string Format(this string value, object arg0, object arg1, object arg2, object arg3)
        {
            try
            {
                return string.Format(value, arg0, arg1, arg2, arg3);
            }
            catch
            {
                return FormatError(value, arg0, arg1, arg2, arg3);
            }
        }

        public static string Format(this string value, params object[] args)
        {
            try
            {
                return string.Format(value, args);
            }
            catch
            {
                return FormatError(value, args);
            }
        }

        private static string FormatError(string value, params object[] args)
        {
            string errorFormat = $"Format Error: \"{value}\": ";
            var b = new StringBuilder(errorFormat, errorFormat.Length + args.Length * 2);
            for (int i = 0; i < args.Length; i++)
            {
                b.Append(args[i]);
                if (i < args.Length)
                    b.Append(',');
            }
            return b.ToString();
        }
    }
}
