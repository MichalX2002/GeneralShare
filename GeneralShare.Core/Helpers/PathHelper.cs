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
        /// Returns <see cref="FileSystemInfo.FullName"/> where
        /// all backslashes are replaced with forward slashes.
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public static string GetFullNameNormalized(this FileSystemInfo info)
        {
            return GetNormalizedPath(info.FullName);
        }

        /// <summary>
        /// Returns a new string where all
        /// backslashes are replaced with forward slashes.
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public static string GetNormalizedPath(string path)
        {
            return path.Replace('\\', '/');
        }
        
        public static string GetRelativePath(string origin, string path)
        {
            int extra = 1;
            if (origin.EndsWith("\\") || origin.EndsWith("/"))
                extra = 0;

            return path.Remove(0, origin.Length + extra);
        }

        public static string GetRelativePath(string origin, FileSystemInfo info)
        {
            return GetRelativePath(origin, info.FullName);
        }

        public static string GetRelativePath(DirectoryInfo origin, FileSystemInfo info)
        {
            return GetRelativePath(origin.FullName, info.FullName);
        }

        public static string GetRelativePath(DirectoryInfo origin, string path)
        {
            return GetRelativePath(origin.FullName, path);
        }
    }
}
