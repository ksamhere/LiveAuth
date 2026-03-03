using LiveAuth.Core.Abstractions;
using LiveAuth.Core.Extensions;
using System.Security.Claims;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.AddLiveAuth(options =>
{
    options.Issuer = builder.Configuration["Jwt:Issuer"] ?? string.Empty;
    options.Audience = builder.Configuration["Jwt:Audience"] ?? string.Empty;
    options.Secret = builder.Configuration["Jwt:Secret"] ?? string.Empty;
});

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});

builder.Services.AddSingleton<ISessionStateStore, FakeSessionStore>();
builder.Services.ConfigureHttpJsonOptions(opts =>
{
    opts.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

var app = builder.Build();

app.UseLiveAuth();

app.MapGet("/secure", (HttpContext context) =>
{
    if (!context.User.Identity!.IsAuthenticated)
    {
        return Results.Unauthorized();
    }

    var roles = context.User.Claims
        .Where(c => c.Type == ClaimTypes.Role)
        .Select(c => c.Value);

    var tenant = context.User.Claims.FirstOrDefault(c => c.Type == "tenant")?.Value;
    var response = new SecureResponse(context.User.Identity?.Name, roles, tenant);
    return Results.Ok(response);
});

app.MapPost("/admin/revoke/{sid}", async (string sid, ISessionStateStore store) =>
{
    await store.RevokeSessionAsync(sid);
    return Results.Ok("Revoked");
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
