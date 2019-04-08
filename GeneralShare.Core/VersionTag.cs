using Newtonsoft.Json;
using System;
using System.IO;
using System.Reflection;

namespace GeneralShare
{
    [JsonObject]
    public class VersionTag
    {
        public const string ResourceFileName = "versiontag.json";
        public const string DefaultValue = "0";

        private static VersionTag _undefined;
        public static VersionTag Undefined
        {
            get
            {
                if(_undefined == null)
                    _undefined = new VersionTag();
                return _undefined;
            }
        }

        [JsonIgnore] public string Value { get; private set; }
        [JsonProperty] public string Major { get; private set; }
        [JsonProperty] public string Minor { get; private set; }
        [JsonProperty] public string Patch { get; private set; }

        [JsonConstructor]
        public VersionTag(string major, string minor, string patch)
        {
            if (string.IsNullOrWhiteSpace(major))
                throw new ArgumentNullException(nameof(major));

            if (string.IsNullOrWhiteSpace(minor))
                minor = DefaultValue;

            if (string.IsNullOrWhiteSpace(patch))
                patch = DefaultValue;

            Major = major;
            Minor = minor;
            Patch = patch;
            CombineVersion();
        }

        public VersionTag(string version)
        {
            string[] split = version.Split('.');
            if (split == null || split.Length != 3)
                throw new ArgumentException(
                    "Could not split version into major, minor and patch.");

            Major = split[0];
            Minor = split[1];
            Patch = split[2];
            CombineVersion();
        }
        
        private VersionTag()
        {
            Major = Minor = Patch = Value = "undefined";
        }

        private void CombineVersion()
        {
            Value = $"{Major}.{Minor}.{Patch}";
        }

        public override string ToString()
        {
            return Value;
        }

        public static VersionTag LoadFrom(Assembly assembly)
        {
            var resourceName = assembly.GetName().Name + "." + ResourceFileName;
            var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null)
                throw new ResourceNotFoundException("Version info cannot be found.", resourceName);

            return JsonUtils.Deserialize<VersionTag>(stream);
        }
    }
}
