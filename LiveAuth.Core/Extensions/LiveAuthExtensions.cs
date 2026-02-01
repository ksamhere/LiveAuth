using LiveAuth.Core.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiveAuth.Core.Extensions
{
    public static class LiveAuthExtensions
    {
        public static IServiceCollection AddLiveAuth(this IServiceCollection services)
        {
            services.AddMemoryCache();
            return services;
        }

        public static IApplicationBuilder UseLiveAuth(this IApplicationBuilder app)
        {
            return app.UseMiddleware<LiveAuthMiddleware>();
        }
    }

}
