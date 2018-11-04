
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;

namespace GeneralShare
{
    public static class JsonUtils
    {
        public const Formatting DEFAULT_FORMATTING = Formatting.None;

        [ThreadStatic]
        private static JsonSerializer __defaultUniqueSerializer;
        private static JsonSerializer DefaultSerializer
        {
            get
            {
                if (__defaultUniqueSerializer == null)
                {
                    __defaultUniqueSerializer = new JsonSerializer
                    {
                        Formatting = DEFAULT_FORMATTING
                    };
                }
                return __defaultUniqueSerializer;
            }
        }

        #region Serialize
        #region Stream-based Serialize
        public static void Serialize<T>(T value, JsonSerializer serializer, Stream stream, bool leaveOpen = false)
        {
            using (var streamWriter = new StreamWriter(stream, Encoding.UTF8, 1024 * 4, leaveOpen))
            using (var jsonWriter = new JsonTextWriter(streamWriter))
            {
                serializer.Serialize(jsonWriter, value);
            }
        }

        public static void Serialize<T>(T value, Formatting formatting, Stream stream, bool leaveOpen = false)
        {
            var serializer = DefaultSerializer;
            serializer.Formatting = formatting;
            Serialize(value, serializer, stream, leaveOpen);
            serializer.Formatting = DEFAULT_FORMATTING;
        }

        public static void Serialize<T>(T value, Stream stream, bool leaveOpen = false)
        {
            Serialize(value, DefaultSerializer, stream, leaveOpen);
        }
        #endregion

        #region Path-based Serialize
        public static void Serialize<T>(T value, JsonSerializer serializer, string path)
        {
            using (var fs = new FileStream(path, FileMode.Create))
                Serialize(value, serializer, fs);
        }

        public static void Serialize<T>(T value, Formatting formatting, string path)
        {
            using (var fs = new FileStream(path, FileMode.Create))
                Serialize(value, formatting, fs);
        }

        public static void Serialize<T>(T value, string path)
        {
            Serialize(value, DefaultSerializer, path);
        }
        #endregion

        #region FileInfo-based Serialize
        public static void Serialize<T>(T value, JsonSerializer serializer, FileInfo file)
        {
            Serialize(value, serializer, file.FullName);
        }

        public static void Serialize<T>(T value, Formatting formatting, FileInfo file)
        {
            Serialize(value, formatting, file.FullName);
        }

        public static void Serialize<T>(T value, FileInfo file)
        {
            Serialize(value, DefaultSerializer, file);
        }
        #endregion
        #endregion

        #region Deserialize
        #region String-based Deserialize 
        public static T DeserializeString<T>(JsonSerializer serializer, string json)
        {
            using (var sr = new StringReader(json))
            using (var jr = new JsonTextReader(sr))
                return serializer.Deserialize<T>(jr);
        }

        public static T DeserializeString<T>(string json)
        {
            return DeserializeString<T>(DefaultSerializer, json);
        }
        #endregion

        #region Stream-based Deserialize
        public static T Deserialize<T>(JsonSerializer serializer, Stream stream, bool leaveOpen = false)
        {
            return serializer.Deserialize<T>(stream, leaveOpen);
        }

        public static T Deserialize<T>(Stream stream, bool leaveOpen = false)
        {
            return Deserialize<T>(DefaultSerializer, stream, leaveOpen);
        }
        #endregion

        #region Path-based Deserialize
        public static T Deserialize<T>(JsonSerializer serializer, string path)
        {
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                return Deserialize<T>(serializer, fs);
        }

        public static T Deserialize<T>(string path)
        {
            return Deserialize<T>(DefaultSerializer, path);
        }
        #endregion

        #region FileInfo-based Deserialize
        public static T Deserialize<T>(JsonSerializer serializer, FileInfo file)
        {
            return Deserialize<T>(serializer, file.FullName);
        }

        public static T Deserialize<T>(FileInfo file)
        {
            return Deserialize<T>(DefaultSerializer, file);
        }
        #endregion
        #endregion
    }
}
