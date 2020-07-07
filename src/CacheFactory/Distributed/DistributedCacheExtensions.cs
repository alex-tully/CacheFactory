using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;

namespace CacheFactory.Distributed
{
    public static class DistributedCacheExtensions
    {
        public static T Get<T>(this IDistributedCache distributedCache, string key)
        {
            if (IsSerializable(typeof(T))) // we will have serialized as bytes
            {
                byte[] data = distributedCache.Get(key);

                if (data == null)
                    return default;

                return FromByteArray<T>(data);
            }
            else // we have serialized as json
            {
                string json = distributedCache.GetString(key);
                if (json == null)
                    return default;

                return FromJson<T>(json);
            }
        }

        public static async Task<T> GetAsync<T>(this IDistributedCache distributedCache, string key, CancellationToken cancellationToken = default)
        {
            if (IsSerializable(typeof(T)))
            {
                byte[] data = await distributedCache.GetAsync(key, cancellationToken);

                if (data == null)
                    return default;

                return FromByteArray<T>(data);
            }
            else
            {
                string json = await distributedCache.GetStringAsync(key);
                if (json == null)
                    return default;

                return FromJson<T>(json);
            }
        }

        public static void Set<T>(this IDistributedCache distributedCache, string key, T value)
        {
            distributedCache.Set<T>(key, value, new DistributedCacheEntryOptions());
        }

        public static void Set<T>(this IDistributedCache distributedCache, string key, T value, DistributedCacheEntryOptions options)
        {
            if (IsSerializable(typeof(T)))
            {
                byte[] serialized = ToByteArray(value);
                distributedCache.Set(key, serialized, options);
            }
            else
            {
                string json = ToJson(value);
                distributedCache.SetString(key, json, options);
            }
        }

        public static Task SetAsync<T>(this IDistributedCache distributedCache, string key, T value, CancellationToken cancellationToken = default)
        {
            return distributedCache.SetAsync<T>(key, value, new DistributedCacheEntryOptions(), cancellationToken);
        }

        public static Task SetAsync<T>(this IDistributedCache distributedCache, string key, T value, DistributedCacheEntryOptions options, CancellationToken cancellationToken = default)
        {
            if (IsSerializable(typeof(T)))
            {
                byte[] serialized = ToByteArray(value);
                return distributedCache.SetAsync(key, serialized, options);
            }
            else
            {
                string json = ToJson(value);
                return distributedCache.SetStringAsync(key, json, options);
            }
        }

        private static bool IsSerializable(Type type)
        {
            // thread safety not massively important here...

            return type.IsSerializable || typeof(ISerializable).IsAssignableFrom(type);
        }

        private static byte[] ToByteArray(object value)
        {
            if (value == null)
                return null;

            var bs = new BinaryFormatter();

            using (MemoryStream ms = new MemoryStream())
            {
                bs.Serialize(ms, value);
                return ms.ToArray();
            }
        }

        private static T FromByteArray<T>(byte[] value)
        {
            BinaryFormatter bs = new BinaryFormatter();

            using (MemoryStream ms = new MemoryStream())
            {
                ms.Write(value, 0, value.Length);
                ms.Seek(0, SeekOrigin.Begin);
                return (T)bs.Deserialize(ms);
            }
        }

        private static string ToJson(object value) => JsonSerializer.Serialize(value);

        private static T FromJson<T>(string value) => JsonSerializer.Deserialize<T>(value);
    }       
}
