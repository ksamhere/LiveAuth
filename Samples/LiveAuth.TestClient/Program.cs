using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

var apiUrl = "http://localhost:5143/secure"; // your minimal API endpoint
var secretKey = "MySuperSecretKeyForHS256MustBe32Byte!";
// same key used in API

var client = new HttpClient();

// Helper to generate JWT
string GenerateJwt(string sid, int ver, bool revoked = false, bool includeClaims = true)
{
    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var claims = includeClaims ? new[]
    {
        new Claim("sub", "user1"),
        new Claim("sid", sid),
        new Claim("ver", ver.ToString())
    } : Array.Empty<Claim>();

    var token = new JwtSecurityToken(
        issuer: "auth.example.com",
        audience: "liveauth-api",
        claims: claims,
        expires: DateTime.UtcNow.AddMinutes(10),
        signingCredentials: creds
    );

    return new JwtSecurityTokenHandler().WriteToken(token);
}

// Edge cases
var testCases = new[]
{
    new { Name = "Valid token", Sid = "S123", Ver = 1, IncludeClaims = true },
    new { Name = "Wrong sid", Sid = "S999", Ver = 1, IncludeClaims = true },
    new { Name = "Wrong version", Sid = "S123", Ver = 99, IncludeClaims = true },
    new { Name = "Missing sid", Sid = "S123", Ver = 1, IncludeClaims = false }
};

// Run tests
foreach (var t in testCases)
{
    var token = GenerateJwt(t.Sid, t.Ver, includeClaims: t.IncludeClaims);
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

    try
    {
        var response = await client.GetAsync(apiUrl);
        var content = await response.Content.ReadAsStringAsync();

        Console.WriteLine($"Test: {t.Name}");
        Console.WriteLine($"Status: {response.StatusCode}");
        Console.WriteLine($"Response: {content}");
        Console.WriteLine(new string('-', 50));
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Test: {t.Name} threw exception: {ex.Message}");
    }
}
