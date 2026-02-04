using LiveAuth.Core.Abstractions;
using LiveAuth.Core.Helper;
using LiveAuth.Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace LiveAuth.Core.Middleware
{
    public class LiveAuthMiddleware
    {
        private readonly RequestDelegate _next;

        public LiveAuthMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context,
            ISessionStateStore store,
            IMemoryCache cache,
            IConfiguration config)
        {
            var auth = context.Request.Headers["Authorization"].ToString();
            if (!auth.StartsWith("Bearer ")) {
                context.Response.StatusCode = 401;
                return;
            }

            var token = auth["Bearer ".Length..];
            var principal = JwtValidator.Validate(token, config);
            if (principal == null)
            {
                context.Response.StatusCode = 401;
                return;
            }
            var sidClaim = principal.FindFirst("sid");
            var verClaim = principal.FindFirst("ver");

            if (sidClaim == null || verClaim == null || !int.TryParse(verClaim.Value, out var ver))
            {
                context.Response.StatusCode = 401;
                return;
            }

             var sid = sidClaim.Value;

            /*
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

            var sid = jwt.Claims.First(c => c.Type == "sid").Value;
            var ver = int.Parse(jwt.Claims.First(c => c.Type == "ver").Value);
            */
            if (!cache.TryGetValue(sid, out SessionState state))
            {
                state = await store.GetAsync(sid);
                if (state != null)
                    cache.Set(sid, state, TimeSpan.FromSeconds(30));
            }

            if (state == null || state.Revoked || state.Version != ver)
            {
                context.Response.StatusCode = 401;
                return;
            }

            var identity = new ClaimsIdentity("LiveAuth");
            identity.AddClaim(new Claim("tenant", state.TenantId));
            identity.AddClaims(state.Roles.Select(r => new Claim(ClaimTypes.Role, r)));
            identity.AddClaims(state.Scopes.Select(s => new Claim("scope", s)));

            context.User = new ClaimsPrincipal(identity);
            await _next(context);
        }
    }

}
