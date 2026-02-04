using LiveAuth.Core.Abstractions;
using LiveAuth.Core.Extensions;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json.Serialization;
using System.Security.Cryptography;


var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.AddMemoryCache();
builder.Services.AddLiveAuth(builder.Configuration);
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});
builder.Services.AddSingleton<ISessionStateStore, FakeSessionStore>();
builder.Services.ConfigureHttpJsonOptions(opts =>
{
    opts.SerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    // Disable source-gen
});

var app = builder.Build();


generateJwt(); // Generate token for testing 

app.UseLiveAuth();

// Minimal API endpoint
app.MapGet("/secure", (HttpContext context) =>
{
    if (!context.User.Identity!.IsAuthenticated)
        return Results.Unauthorized();

    var roles = context.User.Claims
        .Where(c => c.Type == ClaimTypes.Role)
        .Select(c => c.Value);

    var tenant = context.User.Claims.FirstOrDefault(c => c.Type == "tenant")?.Value;
    var response = new SecureResponse(context.User.Identity?.Name, roles, tenant);
    return Results.Ok(response);
});

app.Run();


void generateJwt()
{
    // 32 bytes = 256 bits
    var keyBytes = new byte[32];
    RandomNumberGenerator.Fill(keyBytes); // cryptographically secure

    // store keyBytes as Base64 in config/secret store, then read and Convert.FromBase64String(...)
    var key = new SymmetricSecurityKey(keyBytes);
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var token = new JwtSecurityToken(
        issuer: "auth.example.com",
        audience: "liveauth-api",
        claims: new[]
        {
            new Claim("sub", "user1"),
            new Claim("sid", "S123"),
            new Claim("ver", "1")
        },
        expires: DateTime.UtcNow.AddHours(1),
        signingCredentials: creds
    );

    var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
    Console.WriteLine(tokenString); //Use this token in the Authorization header as: Bearer {tokenString} in the postman or http client to test the /secure endpoint 
}

public record Todo(int Id, string? Title, DateOnly? DueBy = null, bool IsComplete = false);

[JsonSerializable(typeof(SecureResponse[]))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{

}
public record SecureResponse(
    string? User,
    IEnumerable<string> Roles,
    string? Tenant
);
