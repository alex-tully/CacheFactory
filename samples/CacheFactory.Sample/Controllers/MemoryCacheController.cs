using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace CacheFactory.Sample.Controllers
{
    [Route("/memory-cache")]
    public class MemoryCacheController : Controller
    {
        private const string Key = "KEY";
        private static int _counter = 0;
        private readonly IMemoryCache _memoryCache;

        public MemoryCacheController(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        [HttpGet("set")]
        public IActionResult Set()
        {
            string value = $"MemoryCacheController {++_counter}";

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
