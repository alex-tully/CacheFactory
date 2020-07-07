using System;
using System.Linq;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using CacheFactory.Distributed;
using CacheFactory.Memory;

namespace CacheFactory
{
    public static class CacheFactoryServiceCollectionExtensions
    {
        const string DefaultSectionName = "cache";

        public static IServiceCollection AddCacheFactory(this IServiceCollection services)
        {
            return services.RegisterAllDependendecies();
        }

        public static IServiceCollection AddCacheFactory(this IServiceCollection services, IConfiguration configuration)
        {
            return services.AddCacheFactory(configuration.GetSection(DefaultSectionName));
        }

        public static IServiceCollection AddCacheFactory(this IServiceCollection services, IConfigurationSection configurationSection)
        {
            // run this first as we want to override the configuration dependency
            services.RegisterAllDependendecies();

            var reader = new ConfigurationReader();
            CacheConfigurations cacheConfigurations = reader.Read(configurationSection);
            services.AddSingleton<CacheConfigurations>(cacheConfigurations);

            return services;
        }

        private static IServiceCollection RegisterAllDependendecies(this IServiceCollection services)
        {
            if (!services.Any(s => s.ServiceType == typeof(IMemoryCache) || s.ServiceType == typeof(IDistributedCache)))
                throw new InvalidOperationException(
                    $"'AddCacheFactory' should be added after {nameof(IMemoryCache)} and {nameof(IDistributedCache)} have been registered");

            // Why do we do this? 
            // Ultimately we want to give users the ability to continue resolving
            // caches through IMemoryCache / IDistributedCache dependencies
            // so their resolution would take place in the factory.
            // What we have to do though is load the currently configured service collection
            // and provide that into our CacheServiceProvider which can provide resolution to those
            // dependencies before we over write them below
            IServiceProvider serviceProvider = services.BuildServiceProvider();
            services.AddSingleton<ICacheServiceProvider>(new CacheServiceProvider(serviceProvider));
            services.AddSingleton<ICacheFactory, CacheFactory>();
            services.AddSingleton<IMemoryCacheFactory, CacheFactory>();
            services.AddSingleton<IDistributedCacheFactory, CacheFactory>();
            services.AddSingleton<CacheConfigurations>(new CacheConfigurations());

            // WE CONFIGURE THIS OVER THE TOP OF ANY EXISTING CACHE REGISTRATIONS
            // WE EXPECT THIS CACHING TO BE ADDED LAST!
            services.AddSingleton<IMemoryCache>(sp => sp.GetService<IMemoryCacheFactory>().CreateMemoryCache("default"));
            services.AddSingleton<IDistributedCache>(
                sp => sp.GetService<IDistributedCacheFactory>().CreateDistributedCache("default"));

            return services;
        }
    }
}
