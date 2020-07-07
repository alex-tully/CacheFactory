using System;
using System.Collections.Generic;

namespace CacheFactory
{
    internal class CacheConfigurations
    {
        private const string OverrideKey = "override";
        private const string DefaultKey = "default";

        private readonly IReadOnlyDictionary<string, CacheConfiguration> _configurations;
        private readonly CacheConfiguration _override;
        private readonly CacheConfiguration _default;

        public CacheConfigurations(IDictionary<string, CacheConfiguration> configurations = null)
        {
            _configurations = configurations == null ? 
                new Dictionary<string, CacheConfiguration>() :
                new Dictionary<string, CacheConfiguration>(configurations, StringComparer.CurrentCultureIgnoreCase);

            _override = _configurations.ContainsKey(OverrideKey) ? _configurations[OverrideKey] : null;
            _default = _configurations.ContainsKey(DefaultKey) ? _configurations[DefaultKey] : CacheConfiguration.Default;
        }

        public CacheConfiguration GetConfiguration(string name)
        {
            if (_override != null)
                return _override;

            if (HasConfiguration(name))
                return _configurations[name];

            return _default;
        }

        internal int Count => _configurations.Count;

        internal bool HasConfiguration(string name) => _configurations.ContainsKey(name);
    }

    public class CacheConfiguration
    {
        public static readonly CacheConfiguration Default = new CacheConfiguration();

        public CacheConfiguration(CacheExpirySettings expiry, bool enabled = true, bool encrypted = false)
        {
            Expiry = expiry ?? throw new ArgumentNullException(nameof(expiry));
            Enabled = enabled;
            Encrypted = encrypted;
        }

        private CacheConfiguration() { }

        public CacheExpirySettings Expiry { get; } = CacheExpirySettings.Default;

        public bool Enabled { get; } = true;

        public bool Encrypted { get; } = false;
    }

    public class CacheExpirySettings
    {
        public static readonly CacheExpirySettings Default = new CacheExpirySettings();

        public CacheExpirySettings(CacheExpiryType type, TimeSpan ttl)
        {
            Type = type;
            TTL = ttl;
        }

        private CacheExpirySettings() { }

        public CacheExpiryType Type { get; } = CacheExpiryType.None;

        /// <summary>
        /// Time to live
        /// </summary>
        /// <remarks>
        /// How long the item should remain in the cache. By default it is indefinite
        /// </remarks>
        public TimeSpan TTL { get; } = TimeSpan.MaxValue;
    }

    public enum CacheExpiryType
    {
        None,
        Absolute,
        Sliding
    }
}
