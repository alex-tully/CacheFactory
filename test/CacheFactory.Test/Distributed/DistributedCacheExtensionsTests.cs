using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Moq;
using CacheFactory.Distributed;
using Xunit;

namespace CacheFactory.Test.Distributed
{
    public class DistributedCacheExtensionsTests
    {
        private const string Json = @"{""name"":""json-test""}";
        private static readonly SerializableTestClass s_testObject = new SerializableTestClass { Name = "serializable-test" };
        private readonly Mock<IDistributedCache> _mockDistributedCache;
        private readonly IDistributedCache _distributedCache;

        public DistributedCacheExtensionsTests()
        {
            _mockDistributedCache = new Mock<IDistributedCache>();

            _mockDistributedCache
                .Setup(dc => dc.Get(It.IsAny<string>()))
                .Returns((byte[])null);

            _mockDistributedCache
                .Setup(dc => dc.Get("json-test"))
                .Returns(Encoding.UTF8.GetBytes(Json));

            _mockDistributedCache
                .Setup(dc => dc.Get("serializable-test"))
                .Returns(s_testObject.ToByteArray());

            _mockDistributedCache
                .Setup(dc => dc.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((byte[])null);

            _mockDistributedCache
                .Setup(dc => dc.GetAsync("json-test", It.IsAny<CancellationToken>()))
                .ReturnsAsync(Encoding.UTF8.GetBytes(Json));

            _mockDistributedCache
                .Setup(dc => dc.GetAsync("serializable-test", It.IsAny<CancellationToken>()))
                .ReturnsAsync(s_testObject.ToByteArray());

            _distributedCache = _mockDistributedCache.Object;
        }

        public class TheGetMethod : DistributedCacheExtensionsTests
        {
            [Fact]
            public void WhenSerializableTypeThenIsReturnedByGet()
            {
                SerializableTestClass actual = _distributedCache.Get<SerializableTestClass>("serializable-test");

                actual.Should().NotBeNull();
                actual.Name.Should().Be("serializable-test");
            }

            [Fact]
            public void WhenNonSerializableTypeThenIsReturnedByGet()
            {
                JsonTestClass actual = _distributedCache.Get<JsonTestClass>("json-test");

                actual.Should().NotBeNull();
                actual.Name.Should().Be("json-test");
            }

            [Fact]
            public void WhenItemWithKeyDoesNotExistsThenReturnsNull()
            {
                string actual = _distributedCache.Get<string>("not-exists");

                actual.Should().BeNull();
            }

            [Fact]
            public void WhenNonSerializableTypeDoesNotExistThenReturnsNull()
            {
                JsonTestClass actual = _distributedCache.Get<JsonTestClass>("not-exists");
                actual.Should().BeNull();
            }
        }

        public class TheGetAsyncMethod : DistributedCacheExtensionsTests
        {
            [Fact]
            public async Task WhenSerializableTypeThenIsReturnedByGetAsync()
            {
                SerializableTestClass actual = await _distributedCache.GetAsync<SerializableTestClass>("serializable-test");

                actual.Should().NotBeNull();
                actual.Name.Should().Be("serializable-test");
            }

            [Fact]
            public async Task WhenNonSerializableTypeThenIsReturnedByGetAsync()
            {
                JsonTestClass actual = await _distributedCache.GetAsync<JsonTestClass>("json-test");

                actual.Should().NotBeNull();
                actual.Name.Should().Be("json-test");
            }

            [Fact]
            public async Task WhenItemWithKeyDoesNotExistsThenReturnsNull()
            {
                string actual = await _distributedCache.GetAsync<string>("not-exists");

                actual.Should().BeNull();
            }

            [Fact]
            public async Task WhenNonSerializableTypeDoesNotExistThenReturnsNull()
            {
                JsonTestClass actual = await _distributedCache.GetAsync<JsonTestClass>("not-exists");
                actual.Should().BeNull();
            }
        }

        public class TheSetMethod : DistributedCacheExtensionsTests
        {
            [Fact]
            public void WhenSerializableTypeThenBytesAreSet()
            {
                SerializableTestClass testClass = new SerializableTestClass { Name = "serializable-test-xxx" };
                byte[] expected = testClass.ToByteArray();

                _distributedCache.Set<SerializableTestClass>("serializable-test", testClass);

                _mockDistributedCache.Verify(
                    dc => dc.Set(
                        "serializable-test",
                        It.Is<byte[]>(actual => VerifyBytes(expected, actual)),
                        It.IsAny<DistributedCacheEntryOptions>()));
            }

            [Fact]
            public void WhenNonSerializableTypeThenJsonBytesAreSet()
            {
                JsonTestClass testClass = new JsonTestClass { Name = "json-test-xxx" };
                byte[] expected = Encoding.UTF8.GetBytes("{\"Name\":\"json-test-xxx\"}");

                _distributedCache.Set<JsonTestClass>("json-test", testClass);

                _mockDistributedCache.Verify(
                    dc => dc.Set(
                        "json-test", 
                        It.Is<byte[]>(actual => VerifyBytes(expected, actual)), 
                        It.IsAny<DistributedCacheEntryOptions>()));
            }

            [Fact]
            public void WhenItemWithKeyDoesNotExistsThenReturnsNull()
            {
                string actual = _distributedCache.Get<string>("not-exists");

                actual.Should().BeNull();
            }
        }

        public class TheSetAsyncMethod : DistributedCacheExtensionsTests
        {
            [Fact]
            public async Task WhenSerializableTypeThenBytesAreSetAsync()
            {
                SerializableTestClass testClass = new SerializableTestClass { Name = "serializable-test-xxx" };
                byte[] expected = testClass.ToByteArray();

                await _distributedCache.SetAsync<SerializableTestClass>("serializable-test", testClass);

                _mockDistributedCache.Verify(
                    dc => dc.SetAsync(
                        "serializable-test",
                        It.Is<byte[]>(actual => VerifyBytes(expected, actual)),
                        It.IsAny<DistributedCacheEntryOptions>(),
                        It.IsAny<CancellationToken>()));
            }

            [Fact]
            public async Task WhenNonSerializableTypeThenJsonBytesAreSetAsync()
            {
                JsonTestClass testClass = new JsonTestClass { Name = "json-test-xxx" };
                byte[] expected = Encoding.UTF8.GetBytes("{\"Name\":\"json-test-xxx\"}");

                await _distributedCache.SetAsync<JsonTestClass>("json-test", testClass);

                _mockDistributedCache.Verify(
                    dc => dc.SetAsync(
                        "json-test",
                        It.Is<byte[]>(actual => VerifyBytes(expected, actual)),
                        It.IsAny<DistributedCacheEntryOptions>(),
                        It.IsAny<CancellationToken>()));
            }

            [Fact]
            public void WhenItemWithKeyDoesNotExistsThenReturnsNull()
            {
                string actual = _distributedCache.Get<string>("not-exists");

                actual.Should().BeNull();
            }
        }

        private bool VerifyBytes(byte[] expected, byte[] actual)
        {
            actual.Should().BeEquivalentTo(expected);
            return true;
        }

        [Serializable]
        public class SerializableTestClass
        {
            public string Name { get; set; }
        }

        public class JsonTestClass
        {
            public string Name { get; set; }
        }
    }
}
