using System;

namespace GeneralShare
{
    public class EnumHelper
    {
        public static bool IsDefined(Type enumType, string name, bool ignoreCase)
        {
            var comparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            foreach (string enumName in Enum.GetNames(enumType))
                if (enumName.Equals(name, comparison))
                    return true;
            return false;
        }
    }
}
