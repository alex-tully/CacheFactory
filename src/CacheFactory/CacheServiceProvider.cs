using System;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace CacheFactory
{
    internal interface ICacheServiceProvider
    {
        IMemoryCache ResolveMemoryCache();

        IDistributedCache ResolveDistributedCache();
    }

    internal class CacheServiceProvider : ICacheServiceProvider
    {
        private readonly IServiceProvider _serviceProvider;

        public CacheServiceProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IMemoryCache ResolveMemoryCache()
        {
            return _serviceProvider.GetService<IMemoryCache>();
        }

        public IDistributedCache ResolveDistributedCache()
        {
            return _serviceProvider.GetService<IDistributedCache>();
        }
    }
}