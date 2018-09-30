
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;

namespace GeneralShare
{
    public static class JsonUtils
    {
        private const Formatting DefaultFormatting = Formatting.None;

        [ThreadStatic]
        private static JsonSerializer __defaultUniqueSerializer;
        private static JsonSerializer DefaultUniqueSerializer
        {
            get
            {
                if (__defaultUniqueSerializer == null)
                {
                    __defaultUniqueSerializer = new JsonSerializer
                    {
                        Formatting = DefaultFormatting
                    };
                }
                return __defaultUniqueSerializer;
            }
        }

        public static void Serialize<T>(T value, JsonSerializer serializer, Stream stream, bool leaveOpen = false)
        {
            using (var streamWriter = new StreamWriter(stream, Encoding.UTF8, 1024 * 8, leaveOpen))
            using (var jsonWriter = new JsonTextWriter(streamWriter))
            {
                serializer.Serialize(jsonWriter, value);
            }
        }

        public static void Serialize<T>(T value, Formatting formatting, Stream stream, bool leaveOpen = false)
        {
            var serializer = DefaultUniqueSerializer;
            serializer.Formatting = formatting;
            Serialize(value, serializer, stream, leaveOpen);
            serializer.Formatting = DefaultFormatting;
        }

        public static void Serialize<T>(T value, Stream stream, bool leaveOpen = false)
        {
            Serialize(value, DefaultUniqueSerializer, stream, leaveOpen);
        }

        public static T Deserialize<T>(JsonSerializer serializer, Stream stream, bool leaveOpen = false)
        {
            return serializer.Deserialize<T>(stream, leaveOpen);
        }

        public static T Deserialize<T>(Stream stream, bool leaveOpen = false)
        {
            return Deserialize<T>(DefaultUniqueSerializer, stream, leaveOpen);
        }
    }
}
