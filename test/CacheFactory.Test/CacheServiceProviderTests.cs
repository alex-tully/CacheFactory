using System;
using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Xunit;

namespace CacheFactory.Test
{
    public class CacheServiceProviderTests
    {
        [Fact]
        public void TheResolveMemoryCacheMethodReturnsIMemoryCacheFromTheServiceProvider()
        {
            var expected = new Mock<IMemoryCache>().Object;
            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(sp => sp.GetService(typeof(IMemoryCache))).Returns(expected);
            var provider = new CacheServiceProvider(mockServiceProvider.Object);

            var actual = provider.ResolveMemoryCache();

            actual.Should().Be(expected);
        }

        [Fact]
        public void TheResolveDistributedCacheMethodReturnsIDistributedCacheFromTheServiceProvider()
        {
            var expected = new Mock<IDistributedCache>().Object;
            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(sp => sp.GetService(typeof(IDistributedCache))).Returns(expected);
            var provider = new CacheServiceProvider(mockServiceProvider.Object);

            var actual = provider.ResolveDistributedCache();

            actual.Should().Be(expected);
        }
    }
}
