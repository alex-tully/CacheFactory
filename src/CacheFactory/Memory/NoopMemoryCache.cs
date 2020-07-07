using System;
using Microsoft.Extensions.Caching.Memory;

namespace CacheFactory.Memory
{
    internal class NoopMemoryCache : IMemoryCache
    {
        private static readonly Lazy<NoopMemoryCache> s_instance =
                new Lazy<NoopMemoryCache>(() => new NoopMemoryCache());
        public static NoopMemoryCache Instance => s_instance.Value;

        private NoopMemoryCache() { }

        public ICacheEntry CreateEntry(object key)
        {
            return new CacheEntry(key);
        }

        public void Dispose()
        {
        }

        public void Remove(object key)
        {
        }

        public bool TryGetValue(object key, out object value)
        {
            value = null;
            return false;
        }
    }
}
