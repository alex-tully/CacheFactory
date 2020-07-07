using System;
using System.Collections.Generic;
using System.Text;
using FluentAssertions;
using Xunit;

namespace CacheFactory.Test
{
    public class ConfigurationReaderTests
    {
        private readonly ConfigurationReader _configurationReader;

        public ConfigurationReaderTests()
        {
            _configurationReader = new ConfigurationReader();
        }

        public class TheReadMethod : ConfigurationReaderTests
        {
            [Fact]
            public void CanProcessASingleCacheItemWithAllPropertiesAndReturnItInCacheConfigurations()
            {
                string json = @"
{
    'cache' : {
        'items': {
            'expiry': {
                'type':'sliding',
                'ttl': '00:00:10'
            },
            'enabled': true,
            'encrypted': true
        }
    }
}";
                var configs = _configurationReader.Read(JsonStringConfigSource.LoadSection(json, "cache"));
                configs.HasConfiguration("items").Should().BeTrue();
                var config = configs.GetConfiguration("items");

                config.Expiry.Type.Should().NotBeNull().And.HaveFlag(CacheExpiryType.Sliding);
                config.Expiry.TTL.Should().Be(TimeSpan.FromSeconds(10));
                config.Enabled.Should().BeTrue();
                config.Encrypted.Should().BeTrue();
            }

            [Fact]
            public void CanProcessASingleCacheItemWithoutExpiryAndReturnItInCacheConfigurations()
            {
                string json = @"
{
    'cache' : {
        'items': {
            'enabled': true,
            'encrypted': true,
        }
    }
}";
                var configs = _configurationReader.Read(JsonStringConfigSource.LoadSection(json, "cache"));
                configs.HasConfiguration("items").Should().BeTrue();
                var config = configs.GetConfiguration("items");

                config.Expiry.Type.Should().Be(CacheExpirySettings.Default.Type);
                config.Expiry.TTL.Should().Be(CacheExpirySettings.Default.TTL);
                config.Enabled.Should().BeTrue();
                config.Encrypted.Should().BeTrue();
            }

            [Fact]
            public void CanProcessASingleCacheItemWithOnlyEncryptedAndReturnItInCacheConfigurations()
            {
                string json = @"
{
    'cache' : {
        'items': {
            'encrypted': true
        }
    }
}";
                var configs = _configurationReader.Read(JsonStringConfigSource.LoadSection(json, "cache"));
                configs.HasConfiguration("items").Should().BeTrue();
                var config = configs.GetConfiguration("items");

                config.Expiry.Type.Should().Be(CacheExpirySettings.Default.Type);
                config.Expiry.TTL.Should().Be(CacheExpirySettings.Default.TTL);
                config.Enabled.Should().Be(CacheConfiguration.Default.Enabled);
                config.Encrypted.Should().BeTrue();
            }

            [Fact]
            public void CanProcessASingleCacheItemWithOnlyEnabledAndReturnItInCacheConfigurations()
            {
                string json = @"
{
    'cache' : {
        'items': {
            'enabled': false
        }
    }
}";
                var configs = _configurationReader.Read(JsonStringConfigSource.LoadSection(json, "cache"));
                configs.HasConfiguration("items").Should().BeTrue();
                var config = configs.GetConfiguration("items");

                config.Expiry.Type.Should().Be(CacheExpirySettings.Default.Type);
                config.Expiry.TTL.Should().Be(CacheExpirySettings.Default.TTL);
                config.Enabled.Should().BeFalse();
                config.Encrypted.Should().Be(CacheConfiguration.Default.Encrypted);
            }

            [Fact]
            public void CanProcessMutlipleCacheConfigurationAndReturnThemInCacheConfigurations()
            {
                string json = @"
{
    'cache' : {
        'items': {
            'expiry': {
                'type':'sliding',
                'ttl': '00:00:10'
            },
            'enabled': true,
            'encrypted': true
        },
        'http': {
            'enabled': true,
            'encrypted': true
        },
        'tcp': {
            'expiry': {
                'type':'sliding',
                'ttl': '00:00:10'
            },
            'enabled': true,
        },
        'ftp': {
            'expiry': {
                'type':'sliding',
                'ttl': '00:00:10'
            },
            'enabled': true,
            'encrypted': true
        }
    }
}";
                var configs = _configurationReader.Read(JsonStringConfigSource.LoadSection(json, "cache"));
                configs.Count.Should().Be(4);
                configs.HasConfiguration("items").Should().BeTrue();
                configs.HasConfiguration("http").Should().BeTrue();
                configs.HasConfiguration("tcp").Should().BeTrue();
                configs.HasConfiguration("ftp").Should().BeTrue();
            }

            [Fact]
            public void WhenConfigurationContainsDuplicatesThenLastEntryIsUsed()
            {
                string json = @"
{
    'cache' : {
        'items': {
            'expiry': {
                'type':'sliding',
                'ttl': '00:00:10'
            },
            'enabled': true,
            'encrypted': false
        },
        'items': {
            'expiry': {
                'type':'absolute',
                'ttl': '01:00:00'
            },
            'enabled': true,
            'encrypted': true
        }
    }
}";
                var configs = _configurationReader.Read(JsonStringConfigSource.LoadSection(json, "cache"));
                configs.HasConfiguration("items").Should().BeTrue();
                var config = configs.GetConfiguration("items");

                config.Expiry.Type.Should().NotBeNull().And.HaveFlag(CacheExpiryType.Absolute);
                config.Expiry.TTL.Should().Be(TimeSpan.FromHours(1));
                config.Enabled.Should().BeTrue();
                config.Encrypted.Should().BeTrue();
            }

            [Fact]
            public void WhenExpiryTypeCannotBeParsedThenThrows()
            {
                string json = @"
{
    'cache' : {
        'items': {
            'expiry': {
                'type':'xxxx',
            },
        }
    }
}";
                Assert.Throws<InvalidOperationException>(
                    () => _configurationReader.Read(JsonStringConfigSource.LoadSection(json, "cache")));
            }

            [Fact]
            public void WhenExpiryTTLCannotBeParsedThenThrows()
            {
                string json = @"
{
    'cache' : {
        'items': {
            'expiry': {
                'ttl':'xxxx',
            },
        }
    }
}";
                Assert.Throws<InvalidOperationException>(
                    () => _configurationReader.Read(JsonStringConfigSource.LoadSection(json, "cache")));
            }
        }

        [Fact]
        public void WhenEnabledCannotBeParsedThenThrows()
        {
            string json = @"
{
    'cache' : {
        'items': {
            'enabled': 'bawk'
        }
    }
}";
            Assert.Throws<InvalidOperationException>(
                () => _configurationReader.Read(JsonStringConfigSource.LoadSection(json, "cache")));
        }

        [Fact]
        public void WhenEncryptedCannotBeParsedThenThrows()
        {
            string json = @"
{
    'cache' : {
        'items': {
            'encrypted': 'bawk'
        }
    }
}";
            Assert.Throws<InvalidOperationException>(
                () => _configurationReader.Read(JsonStringConfigSource.LoadSection(json, "cache")));
        }
    }
}
