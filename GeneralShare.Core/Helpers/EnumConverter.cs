using System;
using System.Linq.Expressions;

namespace GeneralShare
{
    public static class EnumConverter<TEnum> where TEnum : Enum
    {
        private static readonly Func<long, TEnum> _convert = GenerateConverter();
        
        public static TEnum Convert(long value)
        {
            return _convert.Invoke(value);
        }

        public static TEnum Convert(ulong value)
        {
            return Convert((long)value);
        }

        private static Func<long, TEnum> GenerateConverter()
        {
            var parameter = Expression.Parameter(typeof(long));
            var conversion = Expression.Convert(parameter, typeof(TEnum));
            var method = Expression.Lambda<Func<long, TEnum>>(conversion, parameter);
            return method.Compile();
        }
    }
}
