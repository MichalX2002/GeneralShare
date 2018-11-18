using Newtonsoft.Json;
using System;

namespace GeneralShare
{
    [JsonObject]
    public class VersionTag
    {
        public const string RESOURCE_NAME = "versiontag.json";
        public const string DEFAULT_SUBVALUE = "0";
        
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
                minor = DEFAULT_SUBVALUE;

            if (string.IsNullOrWhiteSpace(patch))
                patch = DEFAULT_SUBVALUE;

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

        /// <summary>
        /// Constructs a new <see cref="VersionTag"/> with an 'undefined' version.
        /// </summary>
        public VersionTag()
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
    }
}
