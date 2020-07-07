using System;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;

namespace CacheFactory.Memory
{
    internal class MemoryCacheDecorator : IMemoryCache
    {
        private readonly IMemoryCache _memoryCache;
        private readonly CacheConfiguration _cacheConfiguration;
        private readonly string _name;

        public MemoryCacheDecorator(
            string name, 
            IMemoryCache memoryCache, 
            CacheConfigurations cacheConfigurations)
        {
            _name = name;
            _memoryCache = memoryCache;
            _cacheConfiguration = cacheConfigurations.GetConfiguration(name);
        }

        public ICacheEntry CreateEntry(object key)
        {
            if (key is null) throw new ArgumentNullException(nameof(key));

            if (_cacheConfiguration.Enabled)
            {
                ICacheEntry entry = _memoryCache.CreateEntry(new CacheEntryKey(_name, key));

                if (_cacheConfiguration.Expiry.Type == CacheExpiryType.Absolute)
                    entry.SetAbsoluteExpiration(_cacheConfiguration.Expiry.TTL);
                else if (_cacheConfiguration.Expiry.Type == CacheExpiryType.Sliding)
                    entry.SetSlidingExpiration(_cacheConfiguration.Expiry.TTL);

                return entry;
            }

            return new CacheEntry(key);
        }

        public void Dispose()
        {
            _memoryCache.Dispose();
        }

        public void Remove(object key)
        {
            if (key is null) throw new ArgumentNullException(nameof(key));

            if (_cacheConfiguration.Enabled)
            {
                _memoryCache.Remove(new CacheEntryKey(_name, key));
            }
        }

        public bool TryGetValue(object key, out object value)
        {
            if (key is null) throw new ArgumentNullException(nameof(key));

            if (_cacheConfiguration.Enabled)
            {
                return _memoryCache.TryGetValue(new CacheEntryKey(_name, key), out value);
            }

            value = null;
            return false;
        }
    }
}