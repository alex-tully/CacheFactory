using System;
using System.Collections.Concurrent;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using CacheFactory.Distributed;
using CacheFactory.Memory;

namespace CacheFactory
{
    public interface ICacheFactory : IMemoryCacheFactory, IDistributedCacheFactory
    {
    }

    internal class CacheFactory : ICacheFactory
    {
        private static readonly ConcurrentDictionary<string, IMemoryCache> s_memoryCacheStore =
            new ConcurrentDictionary<string, IMemoryCache>();
        private static readonly ConcurrentDictionary<string, IDistributedCache> s_distributedCacheStore =
            new ConcurrentDictionary<string, IDistributedCache>();

        private readonly ICacheServiceProvider _cacheServiceProvider;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<CacheFactory> _logger;

        public CacheFactory(
            ICacheServiceProvider cacheServiceProvider, 
            IServiceProvider serviceProvider,
            ILogger<CacheFactory> logger)
        {
            _cacheServiceProvider = cacheServiceProvider;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public IMemoryCache CreateMemoryCache(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("'name' cannot be null or empty", nameof(name));

            return s_memoryCacheStore.GetOrAdd(name, (cacheName) =>
            {
                IMemoryCache memoryCache = _cacheServiceProvider.ResolveMemoryCache();

                if (memoryCache == null)
                {
                    _logger.LogInformation($"Loaded '{nameof(NoopMemoryCache)}' for named cache '{{cache}}'", cacheName);
                    return NoopMemoryCache.Instance;
                }
                else
                {
                    CacheConfigurations cacheConfigurations =
                        _serviceProvider.GetService(typeof(CacheConfigurations)) as CacheConfigurations;

                    return new MemoryCacheDecorator(cacheName, memoryCache, cacheConfigurations ?? new CacheConfigurations());
                }
            });
        }

        public IDistributedCache CreateDistributedCache(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("'name' cannot be null or empty", nameof(name));

            return s_distributedCacheStore.GetOrAdd(name, (cacheName) =>
            {
                IDistributedCache distributedCache = _cacheServiceProvider.ResolveDistributedCache();

                if (distributedCache == null)
                {
                    _logger.LogInformation($"Loaded '{nameof(NoopDistributedCache)}' for named cache '{{cache}}'", cacheName);
                    return NoopDistributedCache.Instance;
                }
                else
                {
                    IDataProtectionProvider dataProtectorProvider =
                        _serviceProvider.GetService<IDataProtectionProvider>();

                    IDataProtector dataProtector;
                    if (dataProtectorProvider == null)
                    {
                        dataProtector = NoopDataProtector.Instance;
                    }
                    else
                    {
                        const string purpose = "OpenWorks Distributed Cache";
                        dataProtector =
                            dataProtectorProvider.CreateProtector(purpose);
                    }

                    CacheConfigurations cacheConfigurations =
                        _serviceProvider.GetService(typeof(CacheConfigurations)) as CacheConfigurations;

                    return new DistributedCacheDecorator(
                        cacheName,
                        distributedCache,
                        dataProtector,
                        cacheConfigurations ?? new CacheConfigurations());
                }
            });
        }
    }
}
