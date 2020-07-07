using Microsoft.Extensions.Caching.Distributed;

namespace CacheFactory.Distributed
{
    public interface IDistributedCacheFactory
    {
        IDistributedCache CreateDistributedCache(string name);
    }
}
