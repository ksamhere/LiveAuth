using LiveAuth.Core.Middleware;
using LiveAuth.Core.Models;
using Microsoft.AspNetCore.Builder;
using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LiveAuth.Core.Extensions
{
    public static class LiveAuthExtensions
    {
        public static IServiceCollection AddLiveAuth(this IServiceCollection services, IConfiguration config)
        {
            services.AddMemoryCache();
            services.Configure<LiveAuthOptions>(config.GetSection("Jwt"));
            return services;
        }

        public static IServiceCollection AddLiveAuth(this IServiceCollection services, Action<LiveAuthOptions> configureOptions)
        {
            services.AddMemoryCache();
            services.Configure(configureOptions);
            return services;
        }

        public static IApplicationBuilder UseLiveAuth(this IApplicationBuilder app)
        {
            return app.UseMiddleware<LiveAuthMiddleware>();
        }
    }
}
