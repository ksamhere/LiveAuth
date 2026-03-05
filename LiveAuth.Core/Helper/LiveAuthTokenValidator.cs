using LiveAuth.Core.Abstractions;
using LiveAuth.Core.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace LiveAuth.Core.Helper
{
    

    internal static class LiveAuthTokenValidator
    {
        public static async Task ValidateAsync(
            TokenValidatedContext context,
            LiveAuthOptions options)
        {
            var reader = context.HttpContext.RequestServices
                .GetRequiredService<ISessionStateReader>();

            var principal = context.Principal!;

            var sid = principal.FindFirst(options.SessionIdClaimType)?.Value;
            var version = principal.FindFirst(options.VersionClaimType)?.Value;

            if (string.IsNullOrWhiteSpace(sid))
            {
                context.Fail("LiveAuth: SessionId claim missing.");
                return;
            }

            var session = await reader.GetSessionAsync(sid);

            if (session == null || session.IsRevoked)
            {
                context.Fail("LiveAuth: Session revoked or not found");
                return;
            }

            if (version != session.Version.ToString())
            {
                context.Fail("LiveAuth: Session version mismatch");
                return;
            }

            if (options.OverrideRoleFromSession)
            {
                OverrideRole(principal, session.Role, options.RoleClaimType);
            }
        }

        private static void OverrideRole(
        ClaimsPrincipal principal,
        string role,
        string roleClaimType)
        {
            var identity = (ClaimsIdentity)principal.Identity!;

            var existingRoleClaims = identity
                .FindAll(roleClaimType)
                .ToList();

            foreach (var claim in existingRoleClaims)
                identity.RemoveClaim(claim);

            identity.AddClaim(new Claim(roleClaimType, role));
        }
    }

}
