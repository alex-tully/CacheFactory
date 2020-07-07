using System;
using System.Collections.Generic;
using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using CacheFactory.Memory;
using Xunit;

namespace CacheFactory.Test.Memory
{
    public class MemoryCacheDecoratorTests
    {
        private Mock<IMemoryCache> _mockMemoryCache;
        private CacheConfigurations _cacheConfigurations;

        public MemoryCacheDecoratorTests()
        {
            _mockMemoryCache = new Mock<IMemoryCache>();
            _cacheConfigurations = new CacheConfigurations(new Dictionary<string, CacheConfiguration>
            {
                ["cache.enabled"] = new CacheConfiguration(CacheExpirySettings.Default, enabled: true),
                ["cache.enabled.absolute"] = new CacheConfiguration(
                    new CacheExpirySettings(CacheExpiryType.Absolute, TimeSpan.FromMinutes(5)), 
                    enabled: true),
                ["cache.enabled.sliding"] = new CacheConfiguration(
                    new CacheExpirySettings(CacheExpiryType.Sliding, TimeSpan.FromMinutes(5)),
                    enabled: true),
                ["cache.disabled"] = new CacheConfiguration(CacheExpirySettings.Default, enabled: false),
            });
        }

        public class TheCreateEntryMethod : MemoryCacheDecoratorTests
        {
            public TheCreateEntryMethod()
            {
                _mockMemoryCache
                    .Setup(c => c.CreateEntry(It.IsAny<object>()))
                    .Returns<object>(o => new CacheEntry(o));
            }

            [Fact]
            public void WhenCacheEnabledThenExecutesOnMemoryCache()
            {
                var memoryCacheDecorator = CreateMemoryCacheDecorator("cache.enabled");

                memoryCacheDecorator.CreateEntry("key");

                _mockMemoryCache.Verify(c => c.CreateEntry(new CacheEntryKey("cache.enabled", "key")), Times.Once);
            }

            [Fact]
            public void WhenCacheEnabledAndAbsoluteExpirySetThenMarkedOnEntry()
            {
                var memoryCacheDecorator = CreateMemoryCacheDecorator("cache.enabled.absolute");

                ICacheEntry entry = memoryCacheDecorator.CreateEntry("key");

                entry.AbsoluteExpirationRelativeToNow.Should().HaveValue().And.Be(TimeSpan.FromMinutes(5));
            }

            [Fact]
            public void WhenCacheEnabledAndSlidingExpirySetThenMarkedOnEntry()
            {
                var memoryCacheDecorator = CreateMemoryCacheDecorator("cache.enabled.sliding");

                ICacheEntry entry = memoryCacheDecorator.CreateEntry("key");

                entry.SlidingExpiration.Should().HaveValue().And.Be(TimeSpan.FromMinutes(5));
            }

            [Fact]
            public void WhenCacheDisabledThenDoesNotExecuteOnMemoryCache()
            {
                var memoryCacheDecorator = CreateMemoryCacheDecorator("cache.disabled");

                memoryCacheDecorator.CreateEntry("key");

                _mockMemoryCache.Verify(c => c.CreateEntry(It.IsAny<object>()), Times.Never);
            }
        }

        public class TheRemoveMethod : MemoryCacheDecoratorTests
        {
            [Fact]
            public void WhenCacheEnabledThenExecutesOnMemoryCache()
            {
                var memoryCacheDecorator = CreateMemoryCacheDecorator("cache.enabled");

                memoryCacheDecorator.Remove("key");

                _mockMemoryCache.Verify(
                    c => c.Remove(
                        new CacheEntryKey("cache.enabled", "key")),
                        Times.Once);
            }

            [Fact]
            public void WhenCacheDisabledThenDoesNotExecuteOnMemoryCache()
            {
                var memoryCacheDecorator = CreateMemoryCacheDecorator("cache.disabled");

                memoryCacheDecorator.Remove("key");

                _mockMemoryCache.Verify(c => c.Remove(It.IsAny<object>()), Times.Never);
            }
        }

        public class TheTryGetValueMethod : MemoryCacheDecoratorTests
        {
            [Fact]
            public void WhenCacheEnabledThenExecutesOnMemoryCache()
            {
                object value;
                var memoryCacheDecorator = CreateMemoryCacheDecorator("cache.enabled");

                memoryCacheDecorator.TryGetValue("key", out value);

                _mockMemoryCache.Verify(
                    c => c.TryGetValue(new CacheEntryKey("cache.enabled", "key"), out value), 
                    Times.Once);
            }

            [Fact]
            public void WhenCacheDisabledThenReturnsTryGetValueResultFromMemoryCache()
            {
                object expected = "expected";
                object actual;
                var memoryCacheDecorator = CreateMemoryCacheDecorator("cache.enabled");
                _mockMemoryCache.Setup(c => c.TryGetValue(new CacheEntryKey("cache.enabled", "key"), out expected))
                    .Returns(true);

                bool found = memoryCacheDecorator.TryGetValue("key", out actual);

                found.Should().BeTrue();
                actual.Should().Be(expected);
            }

            [Fact]
            public void WhenCacheDisabledThenDoesNotExecuteOnMemoryCache()
            {
                object value;
                var memoryCacheDecorator = CreateMemoryCacheDecorator("cache.disabled");

                memoryCacheDecorator.TryGetValue("key", out value);

                _mockMemoryCache.Verify(c => c.TryGetValue(It.IsAny<object>(), out value), Times.Never);
            }

            [Fact]
            public void WhenCacheDisabledThenReturnsFalseAndNullValue()
            {
                object value;
                var memoryCacheDecorator = CreateMemoryCacheDecorator("cache.disabled");

                bool found = memoryCacheDecorator.TryGetValue("key", out value);

                found.Should().BeFalse();
                value.Should().BeNull();
            }
        }

        public class TheDisposeMethod : MemoryCacheDecoratorTests
        {
            [Fact]
            public void DisposesTheMemoryCache()
            {
                var memoryCacheDecorator = CreateMemoryCacheDecorator();

                memoryCacheDecorator.Dispose();

                _mockMemoryCache.Verify(c => c.Dispose(), Times.Once);
            }
        }

        private MemoryCacheDecorator CreateMemoryCacheDecorator(string name = nameof(MemoryCacheDecoratorTests))
        {
            return new MemoryCacheDecorator(name, _mockMemoryCache.Object, _cacheConfigurations);
        }
    }
}
