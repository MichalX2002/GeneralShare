using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;

namespace GeneralShare.Helpers
{
    public static class SecureStringHelper
    {
        public static bool SecureEquals(this SecureString value, SecureString comparison)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            if (comparison == null)
                throw new ArgumentNullException(nameof(comparison));

            if (value.Length != comparison.Length)
                return false;

            IntPtr bstr1 = IntPtr.Zero;
            IntPtr bstr2 = IntPtr.Zero;

            RuntimeHelpers.PrepareConstrainedRegions();
            
            try
            {
                bstr1 = Marshal.SecureStringToBSTR(value);
                bstr2 = Marshal.SecureStringToBSTR(comparison);

                unsafe
                {
                    char* ptr1 = (char*)bstr1.ToPointer();
                    char* ptr2 = (char*)bstr2.ToPointer();

                    for (; *ptr1 != 0 && *ptr2 != 0; ++ptr1, ++ptr2)
                    {
                        if (*ptr1 != *ptr2)
                            return false;
                    }
                }

                return true;
            }
            finally
            {
                if (bstr1 != IntPtr.Zero)
                    Marshal.ZeroFreeBSTR(bstr1);

                if (bstr2 != IntPtr.Zero)
                    Marshal.ZeroFreeBSTR(bstr2);
            }
        }
    }
}
