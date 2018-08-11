using System;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace GeneralShare
{
    public static class EnumConverter<TEnum> where TEnum : Enum
    {
        private static readonly Func<long, TEnum> _convert = GenerateConverter();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TEnum Convert(long value)
        {
            return _convert.Invoke(value);
        }

        private static Func<long, TEnum> GenerateConverter()
        {
            var parameter = Expression.Parameter(typeof(long));
            var conversion = Expression.ConvertChecked(parameter, typeof(TEnum));
            var method = Expression.Lambda<Func<long, TEnum>>(conversion, parameter);
            return method.Compile();
        }
    }
}
