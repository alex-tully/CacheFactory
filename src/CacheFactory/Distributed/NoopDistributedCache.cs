using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;

namespace CacheFactory.Distributed
{
    internal class NoopDistributedCache : IDistributedCache
    {
        private static readonly byte[] EmptyBytes = new byte[0];
        private static readonly Lazy<NoopDistributedCache> s_instance =
            new Lazy<NoopDistributedCache>(() => new NoopDistributedCache());
        public static NoopDistributedCache Instance => s_instance.Value;

        private NoopDistributedCache() { }

        public byte[] Get(string key)
        {
            return EmptyBytes;
        }

        public Task<byte[]> GetAsync(string key, CancellationToken token = default(CancellationToken))
        {
            return Task.FromResult(EmptyBytes);
        }

        public void Refresh(string key)
        {
        }

        public Task RefreshAsync(string key, CancellationToken token = default(CancellationToken))
        {
            return Task.CompletedTask;
        }

        public void Remove(string key)
        {
        }

        public Task RemoveAsync(string key, CancellationToken token = default(CancellationToken))
        {
            return Task.CompletedTask;
        }

        public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
        {
        }

        public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default(CancellationToken))
        {
            return Task.CompletedTask;
        }
    }
}
