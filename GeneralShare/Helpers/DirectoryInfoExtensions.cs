using System.IO;

namespace GeneralShare
{
    public static class DirectoryInfoExtensions
    {
        public static bool Contains(this DirectoryInfo info, string itemName)
        {
            string itemPath = Path.Combine(info.FullName, itemName);
            return Directory.Exists(itemPath) || File.Exists(itemPath);
        }
    }
}
