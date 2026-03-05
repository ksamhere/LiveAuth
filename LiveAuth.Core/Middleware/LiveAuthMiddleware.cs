using LiveAuth.Core.Abstractions;
using LiveAuth.Core.Helper;
using LiveAuth.Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace LiveAuth.Core.Middleware
{
    //public class LiveAuthMiddleware
    //{
    //    private readonly RequestDelegate _next;
    //    private readonly LiveAuthOptions _options;

    //    public LiveAuthMiddleware(RequestDelegate next, IOptions<LiveAuthOptions> options)
    //    {
    //        _next = next;
    //        _options = options.Value;
    //    }

    //    public async Task InvokeAsync(HttpContext context,
    //        ISessionStateStore store,
    //        IMemoryCache cache)
    //    {
    //        var auth = context.Request.Headers["Authorization"].ToString();
    //        if (!auth.StartsWith("Bearer "))
    //        {
    //            context.Response.StatusCode = 401;
    //            return;
    //        }

    //        var token = auth["Bearer ".Length..];
    //        var principal = JwtValidator.Validate(token, _options);
    //        if (principal == null)
    //        {
    //            context.Response.StatusCode = 401;
    //            return;
    //        }

    //        var sidClaim = principal.FindFirst("sid");
    //        var verClaim = principal.FindFirst("ver");

    //        if (sidClaim == null || verClaim == null || !int.TryParse(verClaim.Value, out var ver))
    //        {
    //            context.Response.StatusCode = 401;
    //            return;
    //        }

    //        var sid = sidClaim.Value;

    //        if (!cache.TryGetValue(sid, out SessionState? state))
    //        {
    //            state = await store.GetSessionAsync(sid);
    //            if (state != null)
    //            {
    //                cache.Set(sid, state, TimeSpan.FromSeconds(30));
    //            }
    //        }

    //        if (state == null || state.IsRevoked || state.Version != ver)
    //        {
    //            context.Response.StatusCode = 401;
    //            return;
    //        }

    //        var identity = new ClaimsIdentity("LiveAuth");
    //        identity.AddClaim(new Claim("tenant", state.TenantId));
    //        identity.AddClaims(state.Roles.Select(r => new Claim(ClaimTypes.Role, r)));
    //        identity.AddClaims(state.Scopes.Select(s => new Claim("scope", s)));

    //        context.User = new ClaimsPrincipal(identity);
    //        await _next(context);
    //    }
    //}
}
