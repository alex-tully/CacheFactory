using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using CacheFactory.Memory;

namespace CacheFactory.Sample.Controllers
{
    [Route("/memory-cache-factory")]
    public class MemoryCacheFactoryController : Controller
    {
        private const string Key = "KEY";
        private static int _counter = 0;
        private readonly IMemoryCache _memoryCache;

        public MemoryCacheFactoryController(IMemoryCacheFactory memoryCacheFactory)
        {
            _memoryCache = memoryCacheFactory.CreateMemoryCache(nameof(MemoryCacheFactoryController));
        }

        [HttpGet("set")]
        public IActionResult Set()
        {
            string value = $"MemoryCacheFactoryController {++_counter}";

            _memoryCache.Set(Key, value);

            return Ok(value);
        }

        [HttpGet("get")]
        public IActionResult Get()
        {
            // we will notice that counter values aren't shared
            string value = _memoryCache.Get<string>(Key);

            return Ok(value);
        }
    }
}
