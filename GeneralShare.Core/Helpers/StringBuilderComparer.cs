using System;
using System.Collections.Generic;
using System.Text;

namespace GeneralShare
{
    public struct StringBuilderComparer : IEqualityComparer<StringBuilder>
    {
        public static readonly StringBuilderComparer Instance;

        static StringBuilderComparer()
        {
            Instance = new StringBuilderComparer();
        }

        public bool Equals(StringBuilder x, StringBuilder y)
        {
            if (x == null)
                return false;

            return x.Equals(y);
        }

        public int GetHashCode(StringBuilder obj)
        {
            throw new NotImplementedException();
        }
    }
}
