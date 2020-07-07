using System;
using FluentAssertions;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using CacheFactory.Distributed;
using CacheFactory.Memory;
using Xunit;

namespace CacheFactory.Test
{
    public class CacheFactoryTests
    {
        private Mock<IServiceProvider> _mockServiceProvider;
        private Mock<ICacheServiceProvider> _mockCacheServiceProvider;
        private Mock<ILogger<CacheFactory>> _mockLogger;
        private CacheFactory _cacheFactory;

        public CacheFactoryTests()
        {
            _mockServiceProvider = new Mock<IServiceProvider>();
            _mockCacheServiceProvider = new Mock<ICacheServiceProvider>();
            _mockLogger = new Mock<ILogger<CacheFactory>>();
            _cacheFactory = new CacheFactory(
                _mockCacheServiceProvider.Object, 
                _mockServiceProvider.Object,
                _mockLogger.Object);
        }

        public class TheCreateMemoryCacheMethod : CacheFactoryTests
        {
            private Mock<IMemoryCache> _mockMemoryCache;

            public TheCreateMemoryCacheMethod()
            {
                _mockMemoryCache = new Mock<IMemoryCache>();
                _mockCacheServiceProvider
                    .Setup(sp => sp.ResolveMemoryCache())
                    .Returns(_mockMemoryCache.Object);
            }

            [Fact]
            public void ReturnsTheSameInstanceForTheSameName()
            {
                var cache1 = _cacheFactory.CreateMemoryCache(nameof(ReturnsTheSameInstanceForTheSameName));
                var cache2 = _cacheFactory.CreateMemoryCache(nameof(ReturnsTheSameInstanceForTheSameName));
                var cache3 = _cacheFactory.CreateMemoryCache(nameof(ReturnsTheSameInstanceForTheSameName));
                var cache4 = _cacheFactory.CreateMemoryCache(nameof(ReturnsTheSameInstanceForTheSameName));
                var cache5 = _cacheFactory.CreateMemoryCache(nameof(ReturnsTheSameInstanceForTheSameName));

                cache2.Should().Be(cache1)
                      .And.Be(cache3)
                      .And.Be(cache4)
                      .And.Be(cache5)
                      .And.BeOfType<MemoryCacheDecorator>();
            }

            [Fact]
            public void LoadsTheCacheConfigurationsThroughTheServiceProvider()
            {
                _cacheFactory.CreateMemoryCache(nameof(LoadsTheCacheConfigurationsThroughTheServiceProvider));

                _mockServiceProvider.Verify(sp => sp.GetService(typeof(CacheConfigurations)), Times.Once);
            }

            [Fact]
            public void ReturnsNoopCacheWhenMemoryCacheCannotBeResolved()
            {
                _mockCacheServiceProvider
                    .Setup(sp => sp.ResolveMemoryCache())
                    .Returns<IMemoryCache>(null);

                var cache1 = _cacheFactory.CreateMemoryCache(nameof(ReturnsNoopCacheWhenMemoryCacheCannotBeResolved));

                cache1.Should().Be(NoopMemoryCache.Instance);
            }
        }

        public class TheCreateDistributedCacheMethod : CacheFactoryTests
        {
            private Mock<IDistributedCache> _mockDistributedCache;

            public TheCreateDistributedCacheMethod()
            {
                _mockDistributedCache = new Mock<IDistributedCache>();
                _mockCacheServiceProvider
                    .Setup(sp => sp.ResolveDistributedCache())
                    .Returns(_mockDistributedCache.Object);
            }

            [Fact]
            public void ReturnsTheSameInstanceForTheSameName()
            {
                var cache1 = _cacheFactory.CreateDistributedCache(nameof(ReturnsTheSameInstanceForTheSameName));
                var cache2 = _cacheFactory.CreateDistributedCache(nameof(ReturnsTheSameInstanceForTheSameName));
                var cache3 = _cacheFactory.CreateDistributedCache(nameof(ReturnsTheSameInstanceForTheSameName));
                var cache4 = _cacheFactory.CreateDistributedCache(nameof(ReturnsTheSameInstanceForTheSameName));
                var cache5 = _cacheFactory.CreateDistributedCache(nameof(ReturnsTheSameInstanceForTheSameName));

                cache2.Should().Be(cache1)
                      .And.Be(cache3)
                      .And.Be(cache4)
                      .And.Be(cache5)
                      .And.BeOfType<DistributedCacheDecorator>();
            }

            [Fact]
            public void LoadsTheCacheConfigurationsThroughTheServiceProvider()
            {
                _cacheFactory.CreateDistributedCache(nameof(LoadsTheCacheConfigurationsThroughTheServiceProvider));

                _mockServiceProvider.Verify(
                    sp => sp.GetService(typeof(CacheConfigurations)));
            }

            [Fact]
            public void LoadsTheDataProtectorProviderThroughTheServiceProvider()
            {
                _cacheFactory.CreateDistributedCache(nameof(LoadsTheDataProtectorProviderThroughTheServiceProvider));

                _mockServiceProvider.Verify(sp => sp.GetService(typeof(IDataProtectionProvider)));
            }

            [Fact]
            public void ReturnsNoopCacheWhenDistributedCacheCannotBeResolved()
            {
                _mockCacheServiceProvider
                    .Setup(sp => sp.ResolveDistributedCache())
                    .Returns<IDistributedCache>(null);

                var cache1 = _cacheFactory.CreateDistributedCache(nameof(ReturnsNoopCacheWhenDistributedCacheCannotBeResolved));

                cache1.Should().Be(NoopDistributedCache.Instance);
            }
        }
    }
}
