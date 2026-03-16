using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LiveAuth.Core.Abstractions;
using LiveAuth.Core.Extensions;
using LiveAuth.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IdleTimeoutSessionStore>();
builder.Services.AddSingleton<ISessionStateReader>(sp => sp.GetRequiredService<IdleTimeoutSessionStore>());

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]!))
        };
    })
    .AddLiveAuth(options =>
    {
        options.OverrideRoleFromSession = true;
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapPost("/login/admin", (IdleTimeoutSessionStore store, IConfiguration configuration) =>
{
    var sessionId = Guid.NewGuid().ToString("N");

    var session = new SessionState
    {
        SessionId = sessionId,
        Role = "Admin",
        Version = 1,
        IsRevoked = false
    };

    store.Add(session);

    var claims = new[]
    {
        new Claim("sub", "admin-user"),
        new Claim("sid", sessionId),
        new Claim("ver", session.Version.ToString()),
        new Claim(ClaimTypes.Role, session.Role)
    };

    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Secret"]!));

    var token = new JwtSecurityToken(
        claims: claims,
        expires: DateTime.UtcNow.AddMinutes(60),
        signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

    return Results.Ok(new
    {
        accessToken = new JwtSecurityTokenHandler().WriteToken(token),
        sessionId,
        idleTimeoutSeconds = store.IdleTimeout.TotalSeconds,
        note = "If no authorized API call is made within idleTimeoutSeconds, LiveAuth will revoke the session on the next request."
    });
});

app.MapGet("/admin", [Authorize(Roles = "Admin")] (HttpContext context) =>
{
    var sid = context.User.FindFirst("sid")?.Value;
    return Results.Ok($"Admin access granted for session {sid}");
});

app.MapGet("/me", [Authorize] (HttpContext context) =>
{
    return Results.Ok(new
    {
        user = context.User.FindFirst("sub")?.Value,
        sid = context.User.FindFirst("sid")?.Value,
        role = context.User.FindFirst(ClaimTypes.Role)?.Value
    });
});

app.MapGet("/session/{sessionId}", (IdleTimeoutSessionStore store, string sessionId) =>
{
    var state = store.GetDebugState(sessionId);
    return state is null ? Results.NotFound() : Results.Ok(state);
});

app.MapPost("/logout/{sessionId}", (IdleTimeoutSessionStore store, string sessionId) =>
{
    if (!store.Revoke(sessionId))
    {
        return Results.NotFound();
    }

    return Results.Ok("Session revoked");
});

app.Run();
