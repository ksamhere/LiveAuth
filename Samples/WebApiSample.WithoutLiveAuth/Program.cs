using LiveAuth.Core.Abstractions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]!)),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddSingleton<ISessionStateReader, FakeSessionStore>();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/secure", (HttpContext context) =>
{
    if (context.User.Identity?.IsAuthenticated != true)
    {
        return Results.Unauthorized();
    }

    var roles = context.User.Claims
        .Where(c => c.Type == ClaimTypes.Role)
        .Select(c => c.Value);

    return Results.Ok(new
    {
        Message = "Token accepted using only JWT claims (stateless baseline).",
        Roles = roles
    });
});

app.MapPost("/admin/revoke/{sid}", async (string sid, ISessionStateStore store) =>
{
    await store.RevokeSessionAsync(sid);
    return Results.Ok($"Session {sid} marked revoked in store, but stateless JWT validation will not enforce it immediately.");
});

app.Run();
