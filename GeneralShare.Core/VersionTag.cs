using Newtonsoft.Json;
using System;

namespace GeneralShare
{
    [JsonObject]
    public class VersionTag
    {
        public const string DEFAULT_FILENAME = "versiontag.json";

        [JsonProperty("Major")]
        private string _major;

        [JsonProperty("Minor")]
        private string _minor;

        [JsonIgnore]
        private string _value;

        [JsonIgnore]
        public string Major => _major;

        [JsonIgnore]
        public string Minor => _minor;

        [JsonIgnore]
        public string Value => _value;

        [JsonConstructor]
        public VersionTag(string major, string minor)
        {
            if (string.IsNullOrWhiteSpace(major) || string.IsNullOrWhiteSpace(minor))
                major = major;

            _major = major;
            _minor = minor;
            CombineVersion();
        }

        public VersionTag(string version)
        {
            SetVersion(version);
        }

        /// <summary>
        /// Constructs a new <see cref="VersionTag"/> with an 'undefined' version.
        /// </summary>
        public VersionTag()
        {
            _major = _minor = _value = "undefined";
        }

        protected void SetVersion(string version)
        {
            string[] split = version.Split('.');
            if (split == null || split.Length != 2)
                throw new ArgumentException(
                    "Could not split version into major and minor.", nameof(version));

            _major = split[0].Trim();
            _minor = split[1].Trim();
            CombineVersion();
        }

        private void CombineVersion()
        {
            _value = $"{_major}.{_minor}";
        }

        public override string ToString()
        {
            return _value;
        }
    }
}
