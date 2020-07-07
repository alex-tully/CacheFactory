using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace CacheFactory
{
    class ConfigurationReader
    {
        public CacheConfigurations Read(IConfigurationSection configuration)
        {
            Dictionary<string, CacheConfiguration> configurations = new Dictionary<string, CacheConfiguration>();

            foreach (var section in configuration.GetChildren())
            {
                configurations[section.Key] = ReadCacheConfiguration(section);
            }

            return new CacheConfigurations(configurations);
        }

        private CacheConfiguration ReadCacheConfiguration(IConfigurationSection section)
        {
            var expirySection = section.GetSection("expiry");
            CacheExpirySettings cacheExpirySettings = CacheExpirySettings.Default;
            if (expirySection.Exists())
            {
                cacheExpirySettings = ReadExpirySection(expirySection);
            }

            bool enabled = CacheConfiguration.Default.Enabled;
            var enabledConfigValue = section.GetSection("enabled").Value;
            if (enabledConfigValue != null)
            {
                if (!bool.TryParse(enabledConfigValue.ToLower(), out enabled))
                {
                    throw new InvalidOperationException($"Unable to read the 'enabled' property => '{section.Path}'");
                }
            }

            bool encrypted = CacheConfiguration.Default.Encrypted;
            var encryptedConfigValue = section.GetSection("encrypted").Value;
            if (encryptedConfigValue != null)
            {
                if (!bool.TryParse(encryptedConfigValue.ToLower(), out encrypted))
                {
                    throw new InvalidOperationException($"Unable to read the 'encrypted' property => '{section.Path}'");
                }
            }

            return new CacheConfiguration(cacheExpirySettings, enabled, encrypted);
        }

        private CacheExpirySettings ReadExpirySection(IConfigurationSection expiry)
        {
            CacheExpiryType cacheExpiryType = CacheExpirySettings.Default.Type;
            TimeSpan ttl = CacheExpirySettings.Default.TTL;

            string typeConfigValue = expiry.GetSection("type").Value;
            if (typeConfigValue != null)
            {
                if (!Enum.TryParse(typeConfigValue, true, out cacheExpiryType))
                {
                    throw new InvalidOperationException($"Unable to read the 'type' property => '{expiry.Path}'");
                }
            }

            string ttlConfigValue = expiry.GetSection("ttl").Value;
            if (ttlConfigValue != null)
            {
                if (!TimeSpan.TryParse(ttlConfigValue, out ttl))
                {
                    throw new InvalidOperationException($"Unable to read the 'ttl' property => '{expiry.Path}'");
                }
            }

            return new CacheExpirySettings(cacheExpiryType, ttl);
        }
    }
}
