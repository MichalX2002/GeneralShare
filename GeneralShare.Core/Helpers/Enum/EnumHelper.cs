using System;

namespace GeneralShare
{
    public class EnumHelper
    {
        public static bool IsNameDefined(Type enumType, string name, bool ignoreCase)
        {
            var comparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            foreach (string enumName in Enum.GetNames(enumType))
                if (enumName.Equals(name, comparison))
                    return true;
            return false;
        }

        public static bool IsValueDefined(Type enumType, object value)
        {
            foreach (object enumValue in Enum.GetValues(enumType))
                if (Equals(enumValue, value))
                    return true;
            return false;
        }

        /// <summary>
        /// byte, sbyte, short, ushort, int, uint, long, ulong
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static bool IsValidType(TypeCode code)
        {
            switch (code)
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                    return true;

                default:
                    return false;
            }
        }
    }
}
