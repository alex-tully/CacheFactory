using System;
using System.Reflection;
using FluentAssertions;
using CacheFactory.Memory;
using Xunit;

namespace CacheFactory.Test.Memory
{
    public class CacheEntryKeyTests
    {
        public class TheGetHashCodeMethod
        {
            [Theory]
            [InlineData("key", 1)]
            [InlineData("key", "test")]
            [InlineData("key", 2139476d)]
            [InlineData("key", BindingFlags.CreateInstance)]
            public void ReturnsSameHashCodeWhenSameKeyAndValue(string name, object key)
            {
                var key1 = new CacheEntryKey(name, key);
                var key2 = new CacheEntryKey(name, key);

                key1.GetHashCode().Should().Be(key2.GetHashCode());
            }

            [Fact]
            public void ReturnsSameHashCodeWhenSameKeyAndComplexValue()
            {
                DateTimeOffset now = DateTimeOffset.Now;
                var key1 = new CacheEntryKey("key-test-xxx", new TestClass { Count = 1, DateCreated = now });
                var key2 = new CacheEntryKey("key-test-xxx", new TestClass { Count = 1, DateCreated = now });

                key1.Equals(key2).Should().BeTrue();
            }

            [Fact]
            public void ReturnsDifferentHashCodeWhenDifferentNameAndKey()
            {
                var key1 = new CacheEntryKey("key.1", 1);
                var key2 = new CacheEntryKey("key.2", 2);

                key1.GetHashCode().Should().NotBe(key2.GetHashCode());
            }

            [Fact]
            public void ReturnsDifferentHashCodeWhenDifferentNameSameKey()
            {
                object keyValue = new object();
                var key1 = new CacheEntryKey("key.1", keyValue);
                var key2 = new CacheEntryKey("key.2", keyValue);

                key1.GetHashCode().Should().NotBe(key2.GetHashCode());
            }

            [Fact]
            public void ReturnsDifferentHashCodeWhenSameNameDifferentKey()
            {
                object keyValue1 = new object();
                object keyValue2 = new object();
                var key1 = new CacheEntryKey("key.1", keyValue1);
                var key2 = new CacheEntryKey("key.1", keyValue2);

                key1.GetHashCode().Should().NotBe(key2.GetHashCode());
            }
        }

        public class TheEqualsMethod
        {
            [Theory]
            [InlineData("key", 1)]
            [InlineData("key", "test")]
            [InlineData("key", 2139476d)]
            [InlineData("key", BindingFlags.CreateInstance)]
            public void ReturnsTrueWhenSameKeyAndValue(string name, object key)
            {
                var key1 = new CacheEntryKey(name, key);
                var key2 = new CacheEntryKey(name, key);

                key1.Equals(key2).Should().BeTrue();
            }

            [Fact]
            public void ReturnsTrueWhenSameKeyAndComplexValue()
            {
                DateTimeOffset now = DateTimeOffset.Now;
                var key1 = new CacheEntryKey("key-test-xxx", new TestClass { Count = 1, DateCreated = now });
                var key2 = new CacheEntryKey("key-test-xxx", new TestClass { Count = 1, DateCreated = now });

                key1.Equals(key2).Should().BeTrue();
            }

            [Fact]
            public void ReturnsFalseWhenDifferentNameAndKey()
            {
                var key1 = new CacheEntryKey("key.1", 1);
                var key2 = new CacheEntryKey("key.2", 2);

                key1.Equals(key2).Should().BeFalse();
            }

            [Fact]
            public void ReturnsFalseWhenDifferentNameSameKey()
            {
                object keyValue = new object();
                var key1 = new CacheEntryKey("key.1", keyValue);
                var key2 = new CacheEntryKey("key.2", keyValue);

                key1.Equals(key2).Should().BeFalse();
            }

            [Fact]
            public void ReturnsFalseWhenSameNameDifferentKey()
            {
                object keyValue1 = new object();
                object keyValue2 = new object();
                var key1 = new CacheEntryKey("key.1", keyValue1);
                var key2 = new CacheEntryKey("key.1", keyValue2);

                key1.Equals(key2).Should().BeFalse();
            }

            [Fact]
            public void ReturnsFalseWhenOneKeyIsNull()
            {
                var key1 = new CacheEntryKey("key.1", new object());
                CacheEntryKey key2 = null;

                key1.Equals(key2).Should().BeFalse();
            }
        }

        public class TestClass
        {
            public int Count { get; set; }

            public DateTimeOffset DateCreated { get; set; }

            public override int GetHashCode()
            {
                return Count.GetHashCode() + DateCreated.GetHashCode();
            }

            public override bool Equals(object obj)
            {
                if (obj is TestClass test)
                {
                    return Count.Equals(test.Count) && DateCreated.Equals(test.DateCreated);
                }

                return false;
            }
        }
    }
}
