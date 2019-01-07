using System.Collections.Generic;
using System.Text;

namespace GeneralShare
{
    public class StringBuilderComparer : IEqualityComparer<StringBuilder>
    {
        public static readonly StringBuilderComparer Instance = new StringBuilderComparer();

        private StringBuilderComparer()
        {
        }

        public bool Equals(StringBuilder x, StringBuilder y)
        {
            if (x == null)
                return y == null;

            return x.Equals(y);
        }

        public int GetHashCode(StringBuilder obj)
        {
            unchecked
            {
                int hash = 17;
                int len = obj.Length;
                for (int i = 0; i < len; i++)
                    hash = hash * 31 + obj[i];
                return hash;
            }
        }
    }
}