using Newtonsoft.Json;
using System.IO;

namespace GeneralShare
{
    public static class JsonExtensions
    {
        public static T Deserialize<T>(this JsonSerializer serializer, Stream stream)
        {
            using (var textReader = new StreamReader(stream))
            using (var jsonReader = new JsonTextReader(textReader))
                return serializer.Deserialize<T>(jsonReader);
        }

        public static T Deserialize<T>(this JsonSerializer serializer, string filePath)
        {
            using (var fs = new FileStream(filePath, FileMode.Open))
                return Deserialize<T>(serializer, fs);
        }
    }
}
