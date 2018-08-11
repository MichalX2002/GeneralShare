using System.IO;

namespace GeneralShare
{
    public static class PathHelper
    {
        private static char[] __invalidFileNameChars;

        private static char[] InvalidFileNameChars
        {
            get
            {
                if (__invalidFileNameChars == null)
                    __invalidFileNameChars = Path.GetInvalidFileNameChars();

                return __invalidFileNameChars;
            }
        }

        public static bool IsValidFileName(string fileName)
        {
            char[] invalidChars = InvalidFileNameChars;
            int invalidCharsLength = invalidChars.Length;
            for (int i = 0, length = fileName.Length; i < length; i++)
            {
                for (int j = 0; j < invalidCharsLength; j++)
                {
                    if (fileName[i] == invalidChars[i])
                        return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Returns <see cref="FileSystemInfo.FullName"/> replacing all
        /// backslashes with forward slashes.
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public static string PathNormalized(this FileSystemInfo info)
        {
            return PathNormalized(info.FullName);
        }

        /// <summary>
        /// Returns the <paramref name="path"/> replacing all
        /// backslashes with forward slashes.
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public static string PathNormalized(this string path)
        {
            return path.Replace('\\', '/');
        }
    }
}
