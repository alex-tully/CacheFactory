using System;
using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using CacheFactory.Distributed;
using CacheFactory.Memory;
using Xunit;

namespace CacheFactory.Test
{
    public class CacheFactoryServiceCollectionExtensionsTests
    {
        [Fact]
        public void RegistersAllRequiredDependencies()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddScoped(typeof(IMemoryCache), sp => new Mock<IMemoryCache>().Object);
            services.AddScoped(typeof(IDistributedCache), sp => new Mock<IDistributedCache>().Object);
            services.AddScoped(typeof(ILogger<CacheFactory>), sp => new Mock<ILogger<CacheFactory>>().Object);

            // Act
            services.AddCacheFactory();

            // Assert
            var serviceProvider = services.BuildServiceProvider();

            serviceProvider.GetService<ICacheServiceProvider>().Should().NotBeNull();
            serviceProvider.GetService<ICacheFactory>().Should().NotBeNull();
            serviceProvider.GetService<IMemoryCacheFactory>().Should().NotBeNull();
            serviceProvider.GetService<IDistributedCacheFactory>().Should().NotBeNull();
            serviceProvider.GetService<IMemoryCache>().Should().NotBeNull();
            serviceProvider.GetService<IDistributedCache>().Should().NotBeNull();
        }

        [Fact]
        public void RegistersAllRequiredDependencies_WithConfiguration()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddScoped(typeof(IMemoryCache), sp => new Mock<IMemoryCache>().Object);
            services.AddScoped(typeof(IDistributedCache), sp => new Mock<IDistributedCache>().Object);
            services.AddScoped(typeof(ILogger<CacheFactory>), sp => new Mock<ILogger<CacheFactory>>().Object);

            // Act
            services.AddCacheFactory(new Mock<IConfigurationSection>().Object);

            // Assert
            var serviceProvider = services.BuildServiceProvider();

            serviceProvider.GetService<ICacheServiceProvider>().Should().NotBeNull();
            serviceProvider.GetService<ICacheFactory>().Should().NotBeNull();
            serviceProvider.GetService<IMemoryCacheFactory>().Should().NotBeNull();
            serviceProvider.GetService<IDistributedCacheFactory>().Should().NotBeNull();
            serviceProvider.GetService<IMemoryCache>().Should().NotBeNull();
            serviceProvider.GetService<IDistributedCache>().Should().NotBeNull();
        }

        [Fact]
        public void RegistersAllRequiredDependenciesWithCorrectScope()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddScoped(typeof(IMemoryCache), sp => new Mock<IMemoryCache>().Object);
            services.AddScoped(typeof(IDistributedCache), sp => new Mock<IDistributedCache>().Object);

            // Act
            services.AddCacheFactory();

            // Assert
            services.Should().Contain(sd => sd.Lifetime == ServiceLifetime.Singleton && sd.ServiceType == typeof(ICacheServiceProvider));
            services.Should().Contain(sd => sd.Lifetime == ServiceLifetime.Singleton && sd.ServiceType == typeof(ICacheFactory));
            services.Should().Contain(sd => sd.Lifetime == ServiceLifetime.Singleton && sd.ServiceType == typeof(IMemoryCacheFactory));
            services.Should().Contain(sd => sd.Lifetime == ServiceLifetime.Singleton && sd.ServiceType == typeof(IDistributedCacheFactory));
            services.Should().Contain(sd => sd.Lifetime == ServiceLifetime.Singleton && sd.ServiceType == typeof(CacheConfigurations));
            services.Should().Contain(sd => sd.Lifetime == ServiceLifetime.Singleton && sd.ServiceType == typeof(IMemoryCache));
            services.Should().Contain(sd => sd.Lifetime == ServiceLifetime.Singleton && sd.ServiceType == typeof(IDistributedCache));
        }

        [Fact]
        public void ThrowsWhenNeitherIMemoryCacheAndIDistributedHaveAlreadyBeenRegistered()
        {
            var services = new ServiceCollection();

            Assert.Throws<InvalidOperationException>(() => services.AddCacheFactory());
        }
    }
}
