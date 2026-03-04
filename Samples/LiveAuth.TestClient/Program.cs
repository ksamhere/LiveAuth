using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Text.Json;

var baseUrl = "http://localhost:5000";

var handler = new HttpClientHandler
{
    ServerCertificateCustomValidationCallback =
        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
};

using var client = new HttpClient(handler)
{
    BaseAddress = new Uri(baseUrl)
};

Console.WriteLine("=====================================");
Console.WriteLine(" LIVEAUTH DEMO - ROLE REVOCATION ");
Console.WriteLine("=====================================\n");

//
// 1️ LOGIN
//
Console.WriteLine("1 Logging in as admin...\n");

var loginResponse = await client.PostAsync("/login/admin", null);
var token = (await loginResponse.Content.ReadAsStringAsync()).Trim('"').Trim();

Console.WriteLine($"Token received:\n{token}\n");

client.DefaultRequestHeaders.Authorization =
    new AuthenticationHeaderValue("Bearer", token);

//
// Extract SID
//
var jwtHandler = new JwtSecurityTokenHandler();
var jwt = jwtHandler.ReadJwtToken(token);

var sid = jwt.Claims.First(c => c.Type == "sid").Value;

Console.WriteLine($"Session ID: {sid}\n");

//
// 2️ CALL /admin BEFORE REVOKE
//
Console.WriteLine("2 Calling /admin BEFORE revoke...");

var adminResponse1 = await client.GetAsync("/admin");

Console.WriteLine($"Status: {adminResponse1.StatusCode}");
Console.WriteLine(await adminResponse1.Content.ReadAsStringAsync());
Console.WriteLine();

//
// 3️ REVOKE ADMIN ROLE
//
//Console.WriteLine("3 Revoking Admin role in session store...\n");

//var revokeResponse = await client.PostAsync($"/revoke/{sid}", null);

//Console.WriteLine($"Revoke Status: {revokeResponse.StatusCode}");
//Console.WriteLine(await revokeResponse.Content.ReadAsStringAsync());
//Console.WriteLine();

//
// 3.a) upgrade the version of the Jwt by calling the upgrade endpoint.
// This simulates the scenario where the client receives a new token with an updated version after role revocation. 
// Call either REVOKE or UPGRADE endpoint to trigger the version change in the session store. The next call to /admin will then fail due to version mismatch, simulating the scenario where the client has an old token after role revocation.

Console.WriteLine("3.a) Force upgrade all sessions version in session store...\n");

var upgradeResponse = await client.PostAsync($"/forceUpgradeAllAsync", null);

Console.WriteLine($"Revoke Status: {upgradeResponse.StatusCode}");
Console.WriteLine(await upgradeResponse.Content.ReadAsStringAsync());
Console.WriteLine();

//
// 4️ CALL /admin AFTER REVOKE
//
Console.WriteLine("4 Calling /admin AFTER revoke...\n");

var adminResponse2 = await client.GetAsync("/admin");

Console.WriteLine($"Status: {adminResponse2.StatusCode}");
Console.WriteLine(await adminResponse2.Content.ReadAsStringAsync());

Console.WriteLine("\n=====================================");
Console.WriteLine(" DEMO COMPLETE ");
Console.WriteLine("=====================================");
Console.Read();
