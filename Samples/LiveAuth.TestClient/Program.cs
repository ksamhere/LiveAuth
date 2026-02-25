using Microsoft.IdentityModel.Tokens;
using System;
using System.Buffers.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Reflection.Metadata;
using System.Security.Claims;
using System.Text;
class Program
{
    const string apiUrl = "http://localhost:5000/secure"; // your minimal API endpoint
    const string baseUrl = "http://localhost:5000/";
    const string secretKey = "MySuperSecretKeyForHS256MustBe32Byte!";
    // same key used in API

    static async Task Main()
    {
        Console.WriteLine("-------------------------Before Revoke-------------------------");
        await Runtests();
        Console.WriteLine("-------------------------Revoking Session-----------------------");
        var adminToken = GenerateJwt("S123",1,includeClaims: true, isAdmin: true);
        await RevokeSession(adminToken, "S123");
        Console.WriteLine("-------------------------After Revoke--------------------------");
        await Runtests();
        Console.ReadLine();
    }

    // Helper to generate JWT
    static string GenerateJwt(string sid, int ver, bool revoked = false, bool includeClaims = true, bool isAdmin = false)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = includeClaims ? new List<Claim>
        {
        new Claim("sub", "user1"),
        new Claim("sid", sid),
        new Claim("ver", ver.ToString())
    } : new List<Claim>();
        if (isAdmin)
        {
            claims.Add(new Claim(ClaimTypes.Role, "Admin"));
        }
        var token = new JwtSecurityToken(
            issuer: "auth.example.com",
            audience: "liveauth-api",
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(10),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

   

    // Run tests
    static async Task Runtests()
    {
        var client = new HttpClient();
        // Edge cases
        var testCases = new[]
        {
    new { Name = "Valid token", Sid = "S123", Ver = 1, IncludeClaims = true },
    new { Name = "Wrong sid", Sid = "S999", Ver = 1, IncludeClaims = true },
    new { Name = "Wrong version", Sid = "S123", Ver = 99, IncludeClaims = true },
    new { Name = "Missing sid", Sid = "S123", Ver = 1, IncludeClaims = false }
};
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
    }

    static async Task RevokeSession(string adminToken, string sid)
    {
        using var client = new HttpClient();

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", adminToken);

        try
        {
            var response = await client.PostAsync($"{baseUrl}admin/revoke/{sid}", null);
            var content = await response.Content.ReadAsStringAsync();

            Console.WriteLine($"Status: {response.StatusCode}");
            Console.WriteLine($"Response: {content}");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Exception:");
            Console.WriteLine(ex.Message);
        }
    }
    
}