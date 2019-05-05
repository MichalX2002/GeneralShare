using System.IO;

namespace GeneralShare
{
    public static class FileExtensions
    {
        public static string GetFullNameWithoutExtension(this FileInfo file)
        {
            int extLength = file.Extension.Length;
            string fullName = file.FullName;

            return fullName.Substring(0, fullName.Length - extLength);
        }

        public static string GetNameWithoutExtension(this FileInfo file)
        {
            int extLength = file.Extension.Length;
            string name = file.Name;

            return name.Substring(0, name.Length - extLength);
        }

        public static double ToReadableLength(this FileInfo file, int decimals, out string suffix)
        {
            return FileSizeHelper.ToReadableLength(file.Length, decimals, out suffix);
        }
    }
}
