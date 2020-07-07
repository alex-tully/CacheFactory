using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Caching.Distributed;
using Moq;
using CacheFactory.Distributed;
using Xunit;

namespace CacheFactory.Test.Distributed
{
    public class DistributedCacheDecoratorTests
    {
        private static readonly CancellationToken s_ct = new CancellationToken();

        private Mock<IDistributedCache> _mockDistributedCache;
        private Mock<IDataProtector> _mockDataProtector;
        private CacheConfigurations _cacheConfigurations;

        public DistributedCacheDecoratorTests()
        {
            _mockDistributedCache = new Mock<IDistributedCache>();
            _mockDataProtector = new Mock<IDataProtector>();
            _mockDataProtector.Setup(p => p.CreateProtector(It.IsAny<string>())).Returns(_mockDataProtector.Object);
            _cacheConfigurations = new CacheConfigurations(new Dictionary<string, CacheConfiguration>
            {
                ["cache.enabled"] = new CacheConfiguration(CacheExpirySettings.Default, enabled: true),
                ["cache.absolute"] = new CacheConfiguration(
                    new CacheExpirySettings(CacheExpiryType.Absolute, TimeSpan.FromMinutes(5)),
                    enabled: true),
                ["cache.sliding"] = new CacheConfiguration(
                    new CacheExpirySettings(CacheExpiryType.Sliding, TimeSpan.FromMinutes(5)),
                    enabled: true),
                ["cache.encrypted"] = new CacheConfiguration(CacheExpirySettings.Default, enabled: true, encrypted: true),
                ["cache.disabled"] = new CacheConfiguration(CacheExpirySettings.Default, enabled: false),
            });
        }

        public class TheGetMethod : DistributedCacheDecoratorTests
        {
            [Fact]
            public void WhenCacheEnabledThenExecutesOnDistributedCacheAndReturnsData()
            {
                byte[] expected = new byte[] { 1, 2 };
                _mockDistributedCache.Setup(c => c.Get("cache.enabled:key")).Returns(expected);
                var distributedCacheDecorator = CreateDistributedCacheDecorator("cache.enabled");

                byte[] data = distributedCacheDecorator.Get("key");

                _mockDistributedCache.Verify(c => c.Get("cache.enabled:key"), Times.Once);
                data.Should().Equal(expected);
            }

            [Fact]
            public void WhenCacheEncryptedThenUnprotectsDataAndReturnsUnprotected()
            {
                byte[] @protected = new byte[] { 1, 2, 3, 4 };
                byte[] expected = new byte[] { 212, 55 };
                var distributedCacheDecorator = CreateDistributedCacheDecorator("cache.encrypted");
                _mockDistributedCache.Setup(c => c.Get("cache.encrypted:key")).Returns(@protected);
                _mockDataProtector.Setup(p => p.Unprotect(@protected)).Returns(expected);

                byte[] data = distributedCacheDecorator.Get("key");

                _mockDataProtector.Verify(p => p.Unprotect(@protected), Times.Once);
                data.Should().Equal(expected);
            }

            [Fact]
            public void WhenCacheEnabledAndEncryptedButCacheReturnsNullThenDoesNotUnprotect()
            {
                var distributedCacheDecorator = CreateDistributedCacheDecorator("cache.enabled.encrypted");
                _mockDistributedCache.Setup(c => c.Get("cache.enabled.encrypted:key")).Returns<byte[]>(null);

                byte[] data = distributedCacheDecorator.Get("key");

                _mockDataProtector.Verify(p => p.Unprotect(It.IsAny<byte[]>()), Times.Never);
                data.Should().BeNull();
            }

            [Fact]
            public void WhenCacheDisabledThenDoesNotExecuteOnDistributedCacheAndReturnsNull()
            {
                var distributedCacheDecorator = CreateDistributedCacheDecorator("cache.disabled");

                byte[] data = distributedCacheDecorator.Get("key");

                _mockDistributedCache.Verify(c => c.Get(It.IsAny<string>()), Times.Never);
                data.Should().BeNull();
            }
        }

        public class TheGetAsyncMethod : DistributedCacheDecoratorTests
        {
            [Fact]
            public async Task WhenCacheEnabledThenExecutesOnDistributedCacheAndReturnsData()
            {
                byte[] expected = new byte[] { 1, 2 };
                _mockDistributedCache.Setup(c => c.GetAsync("cache.enabled:key", s_ct)).ReturnsAsync(expected);
                var distributedCacheDecorator = CreateDistributedCacheDecorator("cache.enabled");

                byte[] data = await distributedCacheDecorator.GetAsync("key", s_ct);

                _mockDistributedCache.Verify(c => c.GetAsync("cache.enabled:key", s_ct), Times.Once);
                data.Should().Equal(expected);
            }

            [Fact]
            public async Task WhenCacheEncryptedThenUnprotectsDataAndReturnsUnprotected()
            {
                byte[] @protected = new byte[] { 1, 2, 3, 4 };
                byte[] expected = new byte[] { 212, 55 };
                var distributedCacheDecorator = CreateDistributedCacheDecorator("cache.encrypted");
                _mockDistributedCache.Setup(c => c.GetAsync("cache.encrypted:key", s_ct)).ReturnsAsync(@protected);
                _mockDataProtector.Setup(p => p.Unprotect(@protected)).Returns(expected);

                byte[] data = await distributedCacheDecorator.GetAsync("key", s_ct);

                _mockDataProtector.Verify(p => p.Unprotect(@protected), Times.Once);
                data.Should().Equal(expected);
            }

            [Fact]
            public async Task WhenCacheEnabledAndEncryptedButCacheReturnsNullThenDoesNotUnprotect()
            {
                var distributedCacheDecorator = CreateDistributedCacheDecorator("cache.enabled.encrypted");
                _mockDistributedCache.Setup(c => c.GetAsync("cache.enabled.encrypted:key", s_ct)).ReturnsAsync((byte[])null);

                byte[] data = await distributedCacheDecorator.GetAsync("key", s_ct);

                _mockDataProtector.Verify(p => p.Unprotect(It.IsAny<byte[]>()), Times.Never);
                data.Should().BeNull();
            }

            [Fact]
            public async Task WhenCacheDisabledThenDoesNotExecuteOnDistributedCacheAndReturnsNull()
            {
                var distributedCacheDecorator = CreateDistributedCacheDecorator("cache.disabled");

                byte[] data = await distributedCacheDecorator.GetAsync("key", s_ct);

                _mockDistributedCache.Verify(c => c.GetAsync(It.IsAny<string>(), s_ct), Times.Never);
                data.Should().BeNull();
            }
        }

        public class TheRefreshMethod : DistributedCacheDecoratorTests
        {
            [Fact]
            public void WhenCacheEnabledThenExecutesOnDistributedCache()
            {
                var distributedCacheDecorator = CreateDistributedCacheDecorator("cache.enabled");

                distributedCacheDecorator.Refresh("key");

                _mockDistributedCache.Verify(c => c.Refresh("cache.enabled:key"), Times.Once);
            }

            [Fact]
            public void WhenCacheDisabledThenDoesNotExecuteOnDistributedCache()
            {
                var distributedCacheDecorator = CreateDistributedCacheDecorator("cache.disabled");

                distributedCacheDecorator.Refresh("key");

                _mockDistributedCache.Verify(c => c.Refresh(It.IsAny<string>()), Times.Never);
            }
        }

        public class TheRefreshAsyncMethod : DistributedCacheDecoratorTests
        {
            [Fact]
            public async Task WhenCacheEnabledThenExecutesOnDistributedCache()
            {
                var distributedCacheDecorator = CreateDistributedCacheDecorator("cache.enabled");

                await distributedCacheDecorator.RefreshAsync("key");

                _mockDistributedCache.Verify(c => c.RefreshAsync("cache.enabled:key", s_ct), Times.Once);
            }

            [Fact]
            public async Task WhenCacheDisabledThenDoesNotExecuteOnDistributedCache()
            {
                var distributedCacheDecorator = CreateDistributedCacheDecorator("cache.disabled");

                await distributedCacheDecorator.RefreshAsync("key");

                _mockDistributedCache.Verify(c => c.RefreshAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
            }
        }

        public class TheRemoveMethod : DistributedCacheDecoratorTests
        {
            [Fact]
            public void WhenCacheEnabledThenExecutesOnDistributedCache()
            {
                var distributedCacheDecorator = CreateDistributedCacheDecorator("cache.enabled");

                distributedCacheDecorator.Remove("key");

                _mockDistributedCache.Verify(c => c.Remove("cache.enabled:key"), Times.Once);
            }

            [Fact]
            public void WhenCacheDisabledThenDoesNotExecuteOnDistributedCache()
            {
                var distributedCacheDecorator = CreateDistributedCacheDecorator("cache.disabled");

                distributedCacheDecorator.Remove("key");

                _mockDistributedCache.Verify(c => c.Remove(It.IsAny<string>()), Times.Never);
            }
        }

        public class TheRemoveAsyncMethod : DistributedCacheDecoratorTests
        {
            [Fact]
            public async Task WhenCacheEnabledThenExecutesOnDistributedCache()
            {
                var distributedCacheDecorator = CreateDistributedCacheDecorator("cache.enabled");

                await distributedCacheDecorator.RemoveAsync("key");

                _mockDistributedCache.Verify(c => c.RemoveAsync("cache.enabled:key", s_ct), Times.Once);
            }

            [Fact]
            public async Task WhenCacheDisabledThenDoesNotExecuteOnDistributedCache()
            {
                var distributedCacheDecorator = CreateDistributedCacheDecorator("cache.disabled");

                await distributedCacheDecorator.RemoveAsync("key");

                _mockDistributedCache.Verify(c => c.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
            }
        }

        public class TheSetMethod : DistributedCacheDecoratorTests
        {
            [Fact]
            public void WhenCacheEnabledThenExecutesOnDistributedCache()
            {
                var data = new byte[] { 23, 35 };
                var options = new DistributedCacheEntryOptions();
                var distributedCacheDecorator = CreateDistributedCacheDecorator("cache.enabled");

                distributedCacheDecorator.Set("key", data, options);

                _mockDistributedCache.Verify(c => c.Set("cache.enabled:key", data, options), Times.Once);
            }

            [Fact]
            public void WhenCacheDisbledThendoesNotExecuteOnDistributedCache()
            {
                var data = new byte[] { 23, 35 };
                var options = new DistributedCacheEntryOptions();
                var distributedCacheDecorator = CreateDistributedCacheDecorator("cache.disabled");

                distributedCacheDecorator.Set("key", data, options);

                _mockDistributedCache.Verify(
                    c => c.Set(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>()), 
                    Times.Never);
            }

            [Fact]
            public void WhenCacheEncryptedThenProtectsValue()
            {
                var data = new byte[] { 23, 35 };
                var @protected = new byte[] { 12, 11, 47, 255 };
                var options = new DistributedCacheEntryOptions();
                _mockDataProtector.Setup(p => p.Protect(data)).Returns(@protected);
                var distributedCacheDecorator = CreateDistributedCacheDecorator("cache.encrypted");

                distributedCacheDecorator.Set("key", data, options);

                _mockDistributedCache.Verify(c => c.Set("cache.encrypted:key", @protected, options), Times.Once);
            }

            [Fact]
            public void WhenCacheHasAbsoluteExpiryThenSetsOptions()
            {
                var data = new byte[] { 23, 35 };
                var options = new DistributedCacheEntryOptions();
                var distributedCacheDecorator = CreateDistributedCacheDecorator("cache.absolute");

                distributedCacheDecorator.Set("key", data, options);

                options.AbsoluteExpirationRelativeToNow.Should().HaveValue().And.Be(TimeSpan.FromMinutes(5));
            }

            [Theory]
            [MemberData(nameof(PresetOptions))]
            public void WhenCacheHasAbsoluteExpiryButOptionsAlreadySetThenDoesNotOverride(DistributedCacheEntryOptions options)
            {
                var data = new byte[] { 23, 35 };
                var distributedCacheDecorator = CreateDistributedCacheDecorator("cache.absolute");

                distributedCacheDecorator.Set("key", data, options);

                if (options.AbsoluteExpirationRelativeToNow.HasValue)
                {
                    options.AbsoluteExpirationRelativeToNow.Should().NotBe(TimeSpan.FromMinutes(5));
                }
            }

            [Fact]
            public void WhenCacheHasSlidingExpiryThenSetsOptions()
            {
                var data = new byte[] { 23, 35 };
                var options = new DistributedCacheEntryOptions();
                var distributedCacheDecorator = CreateDistributedCacheDecorator("cache.sliding");

                distributedCacheDecorator.Set("key", data, options);

                options.SlidingExpiration.Should().HaveValue().And.Be(TimeSpan.FromMinutes(5));
            }

            [Theory]
            [MemberData(nameof(PresetOptions))]
            public void WhenCacheHasSlidingExpiryButOptionsAlreadySetThenDoesNotOverride(DistributedCacheEntryOptions options)
            {
                var data = new byte[] { 23, 35 };
                var distributedCacheDecorator = CreateDistributedCacheDecorator("cache.sliding");

                distributedCacheDecorator.Set("key", data, options);

                if (options.SlidingExpiration.HasValue)
                {
                    options.SlidingExpiration.Should().NotBe(TimeSpan.FromMinutes(5));
                }
            }
        }

        public class TheSetAsyncMethod : DistributedCacheDecoratorTests
        {
            [Fact]
            public async Task WhenCacheEnabledThenExecutesOnDistributedCache()
            {
                var data = new byte[] { 23, 35 };
                var options = new DistributedCacheEntryOptions();
                var distributedCacheDecorator = CreateDistributedCacheDecorator("cache.enabled");

                await distributedCacheDecorator.SetAsync("key", data, options, s_ct);

                _mockDistributedCache.Verify(c => c.SetAsync("cache.enabled:key", data, options, s_ct), Times.Once);
            }

            [Fact]
            public async Task WhenCacheDisbledThendoesNotExecuteOnDistributedCache()
            {
                var data = new byte[] { 23, 35 };
                var options = new DistributedCacheEntryOptions();
                var distributedCacheDecorator = CreateDistributedCacheDecorator("cache.disabled");

                await distributedCacheDecorator.SetAsync("key", data, options, s_ct);

                _mockDistributedCache.Verify(
                    c => c.SetAsync(
                        It.IsAny<string>(), 
                        It.IsAny<byte[]>(), 
                        It.IsAny<DistributedCacheEntryOptions>(), 
                        It.IsAny<CancellationToken>()),
                    Times.Never);
            }

            [Fact]
            public async Task WhenCacheEncryptedThenProtectsValue()
            {
                var data = new byte[] { 23, 35 };
                var @protected = new byte[] { 12, 11, 47, 255 };
                var options = new DistributedCacheEntryOptions();
                _mockDataProtector.Setup(p => p.Protect(data)).Returns(@protected);
                var distributedCacheDecorator = CreateDistributedCacheDecorator("cache.encrypted");

                await distributedCacheDecorator.SetAsync("key", data, options, s_ct);

                _mockDistributedCache.Verify(c => c.SetAsync("cache.encrypted:key", @protected, options, s_ct), Times.Once);
            }

            [Fact]
            public async Task WhenCacheHasAbsoluteExpiryThenSetAsyncsOptions()
            {
                var data = new byte[] { 23, 35 };
                var options = new DistributedCacheEntryOptions();
                var distributedCacheDecorator = CreateDistributedCacheDecorator("cache.absolute");

                await distributedCacheDecorator.SetAsync("key", data, options, s_ct);

                options.AbsoluteExpirationRelativeToNow.Should().HaveValue().And.Be(TimeSpan.FromMinutes(5));
            }

            [Theory]
            [MemberData(nameof(PresetOptions))]
            public async Task WhenCacheHasAbsoluteExpiryButOptionsAlreadySetAsyncThenDoesNotOverride(DistributedCacheEntryOptions options)
            {
                var data = new byte[] { 23, 35 };
                var distributedCacheDecorator = CreateDistributedCacheDecorator("cache.absolute");

                await distributedCacheDecorator.SetAsync("key", data, options, s_ct);

                if (options.AbsoluteExpirationRelativeToNow.HasValue)
                {
                    options.AbsoluteExpirationRelativeToNow.Should().NotBe(TimeSpan.FromMinutes(5));
                }
            }

            [Fact]
            public async Task WhenCacheHasSlidingExpiryThenSetAsyncsOptions()
            {
                var data = new byte[] { 23, 35 };
                var options = new DistributedCacheEntryOptions();
                var distributedCacheDecorator = CreateDistributedCacheDecorator("cache.sliding");

                await distributedCacheDecorator.SetAsync("key", data, options, s_ct);

                options.SlidingExpiration.Should().HaveValue().And.Be(TimeSpan.FromMinutes(5));
            }

            [Theory]
            [MemberData(nameof(PresetOptions))]
            public async Task WhenCacheHasSlidingExpiryButOptionsAlreadySetAsyncThenDoesNotOverride(DistributedCacheEntryOptions options)
            {
                var data = new byte[] { 23, 35 };
                var distributedCacheDecorator = CreateDistributedCacheDecorator("cache.sliding");

                await distributedCacheDecorator.SetAsync("key", data, options, s_ct);

                if (options.SlidingExpiration.HasValue)
                {
                    options.SlidingExpiration.Should().NotBe(TimeSpan.FromMinutes(5));
                }
            }
        }

        public static TheoryData<DistributedCacheEntryOptions> PresetOptions()
        {
            var theoryData = new TheoryData<DistributedCacheEntryOptions>();

            theoryData.Add(new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30) });
            theoryData.Add(new DistributedCacheEntryOptions { SlidingExpiration = TimeSpan.FromMinutes(30) });
            theoryData.Add(new DistributedCacheEntryOptions { AbsoluteExpiration = DateTimeOffset.Now.AddDays(1) });

            return theoryData;
        }

        private DistributedCacheDecorator CreateDistributedCacheDecorator(string name = nameof(DistributedCacheDecoratorTests))
        {
            return new DistributedCacheDecorator(
                name, 
                _mockDistributedCache.Object, 
                _mockDataProtector.Object, 
                _cacheConfigurations);
        }
    }
}
