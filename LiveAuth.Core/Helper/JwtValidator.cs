using LiveAuth.Core.Models;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace LiveAuth.Core.Helper
{
    public static class JwtValidator
    {
        public static ClaimsPrincipal? Validate(string token, LiveAuthOptions options)
        {
            if (string.IsNullOrWhiteSpace(options.Secret)
                || string.IsNullOrWhiteSpace(options.Issuer)
                || string.IsNullOrWhiteSpace(options.Audience))
            {
                return null;
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.Secret));

            var parameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateIssuer = true,
                ValidIssuer = options.Issuer,
                ValidateAudience = true,
                ValidAudience = options.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromSeconds(30)
            };

            var handler = new JwtSecurityTokenHandler();

            try
            {
                return handler.ValidateToken(token, parameters, out _);
            }
            catch
            {
                return null;
            }
        }
    }
}
