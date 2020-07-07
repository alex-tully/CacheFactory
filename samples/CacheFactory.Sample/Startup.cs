using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace CacheFactory.Sample
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            // WE MAKE SURE THAT WE ADD THE MEMORY CACHE FIRST
            services.AddMemoryCache().AddCacheFactory();

            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseMvc();
        }
    }
}
