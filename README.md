# CacheFactory

## What is it?

The CacheFactory library offers configurable named caches within an application. We should strive to use this for caching throughout the ecosystem so that we have a consistent caching story.

### Why make it?

So that we could have individually configurable caches for the entire application

## Getting started

Register within `Startup.ConfigureServices`.

```csharp
public class Startup
{
    ...
    public void ConfigureServices(IServiceCollection services)
    {
        // WE MAKE SURE THAT WE ADD THE MEMORY CACHE FIRST
        services.AddMemoryCache().AddCacheFactory();
    }
    ...
}
```

> **NOTE: YOU MUST REGISTER `AddCacheFactory()` AFTER ALL OTHER CACHING REGISTRATIONS**

There are 3 types of cache factory that you can inject into your dependents; `IMemoryCacheFactory`, `IDistributedCacheFactory` and `ICacheFactory`. Alternatively you can just use `IMemoryCache` and `IDistributedCache` as you do today.

### Using IMemoryCacheFactory

```csharp
public class MyService(IMemoryCacheFactory memoryCacheFactory) {
    _memoryCache = memoryCacheFactory.CreateMemoryCache("myservice.cache");
}
```

### Using IDistributedCacheFactory

```csharp
public class MyService(IDistributedCacheFactory distributedCacheFactory) {
    _distributedCache = distributedCacheFactory.CreateDistributedCache("myservice.cache");
}
```

### Using ICacheFactory

```csharp
public class MyService(IDistributedCacheFactory distributedCacheFactory) {
    _memoryCache = memoryCacheFactory.CreateMemoryCache("myservice.cache");
    _distributedCache = distributedCacheFactory.CreateDistributedCache("myservice.cache");
}
```

### Using IMemoryCache and IDistributedCache

You can continue to use these with no change to existing functionality. These caches will now be resolved through the factory and will use configured defaults.

## Configuration

We offer the ability to configure each named cache, set cache defaults and override all cache configuration.

A named configuration looks like:

```json
{
    'cache' : {
        'accounts': {
            'expiry': {
                'type':'sliding',
                'ttl': '00:00:10'
            },
            'enabled': true,
            'encrypted': true
        }
    }
}
```

Here you can see that you can configure:

- Expiry - The expiry setting for a cache
  - type - The type of expiration to use `None`, `Sliding` and `Absolute`
  - ttl - How long the item should be cached for
- Enabled - Is this named cache enabled
- Encrypted - Is the data in the cache encrypted (only available in distributed cache)

### default settings

The library has a set of built in defaults but any or all of these can be changed via configuration, only the elements you set will be changed:

```json
{
    'cache' : {
        'default': {
            'expiry': {
                'type':'sliding',
                'ttl': '00:00:10'
            },
            'enabled': true,
            'encrypted': true
        }
    }
}
```

> NOTE: If a named cache doesn't have a configuration it will always fall back to default settings

### override settings

You can override every cache configuration including default by setting the override in the configuration:

```json
{
    'cache' : {
        'override': {
            'enabled': false
        }
    }
}
```

> NOTE: If an override is set it's settings will be used for ALL resolved caches

## Encrypted cache entries

Encrypting cache entries is only available on the distributed cache as that persists raw bytes out to Redis / Sql / whatever. We felt it necessary to offer security here and that is achieved through the .net core data protection library. ([Github](https://github.com/aspnet/dataprotection)) ([Nuget](https://www.nuget.org/packages/Microsoft.AspNetCore.DataProtection))

To enable within your application you will need to register data protection within startup:

```csharp
public class Startup
{
    ...
    public void ConfigureServices(IServiceCollection services)
    {
        // WE MAKE SURE THAT WE ADD THE MEMORY CACHE FIRST
        services.AddMemoryCache().AddCacheFactory();
        services.AddDataProtection();
    }
    ...
}
```