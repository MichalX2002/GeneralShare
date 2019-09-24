using Newtonsoft.Json;
using System.IO;
using System.Text;

namespace GeneralShare
{
    public static class JsonExtensions
    {
        public static T Deserialize<T>(this JsonSerializer serializer, Stream stream, bool leaveOpen = false)
        {
            using (var textReader = new StreamReader(stream, Encoding.UTF8, true, 1024 * 8, leaveOpen))
            using (var jsonReader = new JsonTextReader(textReader))
                return serializer.Deserialize<T>(jsonReader);
        }

        public static T Deserialize<T>(this JsonSerializer serializer, string filePath)
        {
            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                return Deserialize<T>(serializer, fs);
        }

        public static T Deserialize<T>(this JsonSerializer serializer, FileInfo file)
        {
            return Deserialize<T>(serializer, file.FullName);
        }
    }
}
