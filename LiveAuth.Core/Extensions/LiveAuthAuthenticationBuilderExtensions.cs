using LiveAuth.Core.Helper;
using LiveAuth.Core.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace LiveAuth.Core.Extensions;

public static class LiveAuthAuthenticationBuilderExtensions
{
    public static AuthenticationBuilder AddLiveAuth(
        this AuthenticationBuilder builder,
        Action<LiveAuthOptions>? configure = null)
    {
        // Configure options
        var options = new LiveAuthOptions();
        configure?.Invoke(options);
        builder.Services.AddSingleton(options);

        // Hook into the existing JWT scheme
        builder.Services.Configure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, jwtOptions =>
        {
            var existingHandler = jwtOptions.Events?.OnTokenValidated;
            jwtOptions.Events ??= new JwtBearerEvents();

            jwtOptions.Events.OnTokenValidated = async context =>
            {
                // Call any existing handler first
                if (existingHandler is not null)
                    await existingHandler(context);

                // Resolve options & validator
                var liveOptions = context.HttpContext.RequestServices
                    .GetRequiredService<LiveAuthOptions>();

                await LiveAuthTokenValidator.ValidateAsync(context, liveOptions);
            };
        });

        return builder;
    }
}

