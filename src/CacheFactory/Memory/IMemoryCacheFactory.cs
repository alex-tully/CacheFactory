using Microsoft.Extensions.Caching.Memory;

namespace CacheFactory.Memory
{
    public interface IMemoryCacheFactory
    {
        IMemoryCache CreateMemoryCache(string name);
    }
}
