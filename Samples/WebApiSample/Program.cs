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
app.MapPost("/admin/revoke/{sid}", (string sid, ISessionStateStore store) =>
{
    if (store is FakeSessionStore memoryStore)
    {
        memoryStore.RevokeSessionAsync(sid);
    }

    return Results.Ok("Revoked");
});
app.Run();

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
