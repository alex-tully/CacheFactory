using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;

namespace CacheFactory.Distributed
{
    internal class DistributedCacheDecorator : IDistributedCache
    {
        private readonly string _name;
        private readonly IDistributedCache _distributedCache;
        private readonly IDataProtector _dataProtector;
        private readonly CacheConfiguration _cacheConfiguration;

        public DistributedCacheDecorator(
            string name, 
            IDistributedCache distributedCache, 
            IDataProtector dataProtector, 
            CacheConfigurations cacheConfigurations)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("'name' cannot be null or empty", nameof(name));

            _name = name;
            _distributedCache = distributedCache;
            _dataProtector = dataProtector.CreateProtector(_name);
            _cacheConfiguration = cacheConfigurations.GetConfiguration(name);
        }

        public byte[] Get(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("'key' cannot be null or empty", nameof(key));

            if (_cacheConfiguration.Enabled)
            {
                byte[] data = _distributedCache.Get(FormatKey(key));
                return Unprotect(data);
            }

            return null;
        }

        public async Task<byte[]> GetAsync(string key, CancellationToken token = default)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("'key' cannot be null or empty", nameof(key));

            if (_cacheConfiguration.Enabled)
            {
                byte[] data = await _distributedCache.GetAsync(FormatKey(key), token).ConfigureAwait(false);
                return Unprotect(data);
            }

            return null;
        }

        public void Refresh(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("'key' cannot be null or empty", nameof(key));

            if (_cacheConfiguration.Enabled)
            {
                _distributedCache.Refresh(FormatKey(key));
            }
        }

        public Task RefreshAsync(string key, CancellationToken token = default)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("'key' cannot be null or empty", nameof(key));

            if (_cacheConfiguration.Enabled)
            {
                return _distributedCache.RefreshAsync(FormatKey(key), token);
            }

            return Task.CompletedTask;
        }

        public void Remove(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("'key' cannot be null or empty", nameof(key));

            if (_cacheConfiguration.Enabled)
            {
                _distributedCache.Remove(FormatKey(key));
            }
        }

        public Task RemoveAsync(string key, CancellationToken token = default)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("'key' cannot be null or empty", nameof(key));

            if (_cacheConfiguration.Enabled)
            {
                return _distributedCache.RemoveAsync(FormatKey(key), token);
            }

            return Task.CompletedTask;
        }

        public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("'key' cannot be null or empty", nameof(key));
            if (value is null) throw new ArgumentNullException(nameof(value));
            if (options is null) throw new ArgumentNullException(nameof(options));

            if (_cacheConfiguration.Enabled)
            {
                ConfigureOptions(options);
                byte[] data = Protect(value);
                _distributedCache.Set(FormatKey(key), data, options);
            }
        }

        public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("'key' cannot be null or empty", nameof(key));
            if (value is null) throw new ArgumentNullException(nameof(value));
            if (options is null) throw new ArgumentNullException(nameof(options));

            if (_cacheConfiguration.Enabled)
            {
                ConfigureOptions(options);
                byte[] data = Protect(value);
                return _distributedCache.SetAsync(FormatKey(key), data, options, token);
            }

            return Task.CompletedTask;
        }

        private byte[] Protect(byte[] unprotected)
        {
            if (_cacheConfiguration.Encrypted)
            {
                return _dataProtector.Protect(unprotected);
            }

            return unprotected;
        }

        private byte[] Unprotect(byte[] @protected)
        {
            if (_cacheConfiguration.Encrypted && @protected != null)
            {
                return _dataProtector.Unprotect(@protected);
            }

            return @protected;
        }

        private void ConfigureOptions(DistributedCacheEntryOptions options)
        {
            if (_cacheConfiguration.Expiry.Type != CacheExpiryType.None &&
                !options.AbsoluteExpiration.HasValue &&
                !options.AbsoluteExpirationRelativeToNow.HasValue &&
                !options.SlidingExpiration.HasValue)
            {
                if (_cacheConfiguration.Expiry.Type == CacheExpiryType.Absolute)
                    options.SetAbsoluteExpiration(_cacheConfiguration.Expiry.TTL);
                else if (_cacheConfiguration.Expiry.Type == CacheExpiryType.Sliding)
                    options.SetSlidingExpiration(_cacheConfiguration.Expiry.TTL);
            }
        }

        private string FormatKey(string key) => $"{_name}:{key}";
    }
}