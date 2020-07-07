using System.Collections.Generic;
using FluentAssertions;
using Xunit;

namespace CacheFactory.Test
{
    public class CacheConfigurationsTests
    {
        public class TheGetConfigurationMethod
        {
            [Fact]
            public void WhenConfigurationExistsThenItIsReturned()
            {
                var expected = new CacheConfiguration(CacheExpirySettings.Default);

                var cacheConfigurations = new CacheConfigurations(new Dictionary<string, CacheConfiguration>
                {
                    ["exists"] = expected
                });

                var actual = cacheConfigurations.GetConfiguration("exists");

                actual.Should().Be(expected);
            }

            [Fact]
            public void WhenConfigurationDoesNotExistThenReturnsDefault()
            {
                var cacheConfigurations = new CacheConfigurations(new Dictionary<string, CacheConfiguration>
                {
                    ["exists"] = new CacheConfiguration(CacheExpirySettings.Default)
                });

                var actual = cacheConfigurations.GetConfiguration("not-exists");

                actual.Should().Be(CacheConfiguration.Default);
            }

            [Fact]
            public void WhenConfigurationExistsButOverridePresentThenReturnsOverride()
            {
                var item = new CacheConfiguration(CacheExpirySettings.Default);
                var @override = new CacheConfiguration(CacheExpirySettings.Default);

                var cacheConfigurations = new CacheConfigurations(new Dictionary<string, CacheConfiguration>
                {
                    ["item"] = item,
                    ["override"] = @override,
                });

                var actual = cacheConfigurations.GetConfiguration("item");

                actual.Should().Be(@override);
            }

            [Fact]
            public void WhenConfigurationDoesNotExistsAndOverridePresentThenReturnsOverrideNotDefault()
            {
                var @override = new CacheConfiguration(CacheExpirySettings.Default);

                var cacheConfigurations = new CacheConfigurations(new Dictionary<string, CacheConfiguration>
                {
                    ["override"] = @override,
                });

                var actual = cacheConfigurations.GetConfiguration("not-exists");

                actual.Should().Be(@override);
                actual.Should().NotBe(CacheConfiguration.Default);
            }

            [Fact]
            public void WhenDefaultPresentThenDoesNotReturnStaticDefault()
            {
                var @default = new CacheConfiguration(CacheExpirySettings.Default);

                var cacheConfigurations = new CacheConfigurations(new Dictionary<string, CacheConfiguration>
                {
                    ["default"] = @default,
                });

                var actual = cacheConfigurations.GetConfiguration("not-exists");

                actual.Should().Be(@default);
                actual.Should().NotBe(CacheConfiguration.Default);
            }
        }
    }
}
