using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace CacheFactory.Sample.Controllers
{
    [Route("/cache-factory")]
    public class CacheFactoryController : Controller
    {
        private const string Key = "KEY";
        private static int _counter = 0;
        private readonly IMemoryCache _memoryCache;

        public CacheFactoryController(ICacheFactory cacheFactory)
        {
            _memoryCache = cacheFactory.CreateMemoryCache(nameof(CacheFactoryController));
        }

        [HttpGet("set")]
        public IActionResult Set()
        {
            string value = $"CacheFactoryController {++_counter}";

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
