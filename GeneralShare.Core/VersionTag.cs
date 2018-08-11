using Newtonsoft.Json;
using System;

namespace GeneralShare
{
    [JsonObject]
    public class VersionTag
    {
        public const string DEFAULT_FILENAME = "versiontag.json";

        [JsonProperty("VersionMajor")]
        private string _versionMajor;

        [JsonProperty("VersionMinor")]
        private string _versionMinor;

        [JsonIgnore]
        private string _version;

        [JsonIgnore]
        public string VersionMajor => _versionMajor;

        [JsonIgnore]
        public string VersionMinor => _versionMinor;

        [JsonIgnore]
        public string Version => _version;

        [JsonConstructor]
        public VersionTag(string versionMajor, string versionMinor)
        {
            _versionMajor = versionMajor;
            _versionMinor = versionMinor;
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
            _version = "undefined";
            _versionMajor = _version;
            _versionMinor = _version;
        }

        protected void SetVersion(string version)
        {
            string[] split = version.Split('.');
            if (split == null || split.Length != 2)
                throw new ArgumentException(
                    "Could not split version into major and minor.", nameof(version));

            _versionMajor = split[0].Trim();
            _versionMinor = split[1].Trim();
            CombineVersion();
        }

        private void CombineVersion()
        {
            _version = $"{_versionMajor}.{_versionMinor}";
        }

        public override string ToString()
        {
            return _version;
        }
    }
}
