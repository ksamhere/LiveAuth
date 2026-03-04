using LiveAuth.Core.Abstractions;
using LiveAuth.Core.Extensions;
using LiveAuth.Core.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using static System.Formats.Asn1.AsnWriter;

var builder = WebApplication.CreateSlimBuilder(args);
int currentVersion = 1; // Simulate token versioning for invalidation

var useLiveAuth = builder.Configuration.GetValue<bool>("UseLiveAuth");

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});

builder.Services.AddSingleton<ISessionStateReader, FakeSessionStore>();
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(
                        builder.Configuration["Jwt:Secret"]))

        };
    });
// Add LiveAuth authentication if enabled in configuration
if (useLiveAuth)
{
    builder.Services.AddAuthentication()
        .AddLiveAuth(options =>
        { 
        options.OverrideRoleFromSession = true;
        });
}
//LiveAuth section ends here

builder.Services.ConfigureHttpJsonOptions(opts =>
{
    opts.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});
builder.Services.AddAuthorization();
var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

//
// LOGIN ENDPOINT
//
app.MapPost("/login/admin", () =>
{
    var sessionId = Guid.NewGuid().ToString();

    var session = new SessionState
    {
        SessionId = sessionId,
        Role = "Admin",
        Version = currentVersion,
        IsRevoked = false
    };

    FakeSessionStore.Add(session);

    var claims = new[]
    {
        new Claim("sub", "admin"),
        new Claim("sid", sessionId),
        new Claim("ver", "1"),
        new Claim(ClaimTypes.Role, "Admin")
    };

    var key = new SymmetricSecurityKey(
        Encoding.UTF8.GetBytes(
            builder.Configuration["Jwt:Secret"]));

    var token = new JwtSecurityToken(
        claims: claims,
        expires: DateTime.UtcNow.AddHours(1),
        signingCredentials:
            new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

    return Results.Ok(new JwtSecurityTokenHandler().WriteToken(token));
});

//
// ADMIN ENDPOINT
//
app.MapGet("/admin", [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")] () =>
{
    return Results.Ok("Admin access granted");
});

//
// REVOKE ADMIN ROLE (Simulate DB change)
//
app.MapPost("/revoke/{sid}", (string sid) =>
{
    var session = new SessionState
    {
        SessionId = sid,
        Role = "User", // downgrade role
        Version = currentVersion,
        IsRevoked = true
    };

    FakeSessionStore.Update(session);

    return Results.Ok("Admin role revoked in session store.");
});
app.MapPost("/forceUpgradeAllAsync", async () =>
{    
    foreach(var session in await FakeSessionStore.GetAllSessionsAsync())
    {
        var newSession = session with { Version = session.Version + 1 };
        FakeSessionStore.Update(newSession);
    }    

    return Results.Ok("All sessions logged out.");
});
app.Run();

[JsonSerializable(typeof(SecureResponse[]))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{
}

public record SecureResponse(
    string? User,
    IEnumerable<string> Roles,
    string? Tenant
);
