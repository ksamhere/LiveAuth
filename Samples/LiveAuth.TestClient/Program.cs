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


await Mainmethod();

async Task Mainmethod()
{
    Console.WriteLine("=====================================");
    Console.WriteLine(" LIVEAUTH DEMO - IDLE TIMEOUT LOGOUT");
    Console.WriteLine("=====================================\n");
    Console.WriteLine("=====================================");
    Console.WriteLine(" Select input to run the demo:");
    Console.WriteLine(" 1. ROLE REVOCATION");
    Console.WriteLine(" 2. IDLE TIMEOUT LOGOUT");
    int input = 0; int.TryParse(Console.ReadLine(), out input);
    if (input == 1)
        await RoleRevocation();
    else if (input == 2)
        await IdleTimeOut();
   
}

async Task RoleRevocation()
{
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
    Console.WriteLine("3 Revoking Admin role in session store...\n");

    var revokeResponse = await client.PostAsync($"/revoke/{sid}", null);

    Console.WriteLine($"Revoke Status: {revokeResponse.StatusCode}");
    Console.WriteLine(await revokeResponse.Content.ReadAsStringAsync());
    Console.WriteLine();

    //
    // 3.a) upgrade the version of the Jwt by calling the upgrade endpoint.
    // This simulates the scenario where the client receives a new token with an updated version after role revocation. 
    // Call either REVOKE or UPGRADE endpoint to trigger the version change in the session store. The next call to /admin will then fail due to version mismatch, simulating the scenario where the client has an old token after role revocation.

    //Console.WriteLine("3.a) Force upgrade all sessions version in session store...\n");

    //var upgradeResponse = await client.PostAsync($"/forceUpgradeAllAsync", null);

    //Console.WriteLine($"Revoke Status: {upgradeResponse.StatusCode}");
    //Console.WriteLine(await upgradeResponse.Content.ReadAsStringAsync());
    //Console.WriteLine();

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
}

async Task IdleTimeOut()
{
    Console.WriteLine("=====================================");
    Console.WriteLine(" LIVEAUTH DEMO - IDLE TIMEOUT LOGOUT");
    Console.WriteLine("=====================================\n");
    Console.WriteLine($"API Base URL: {baseUrl}\n");

    Console.WriteLine("1) Logging in as admin...");
    var loginResponse = await client.PostAsync("/login/admin", content: null);
    if (!loginResponse.IsSuccessStatusCode)
    {
        Console.WriteLine($"Login failed: {(int)loginResponse.StatusCode} {loginResponse.ReasonPhrase}");
        Console.WriteLine(await loginResponse.Content.ReadAsStringAsync());
        return;
    }

    var loginJson = await loginResponse.Content.ReadAsStringAsync();
    var login = JsonSerializer.Deserialize<LoginResponse>(loginJson, new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    });

    if (login is null || string.IsNullOrWhiteSpace(login.AccessToken) || string.IsNullOrWhiteSpace(login.SessionId))
    {
        Console.WriteLine("Login response was missing accessToken/sessionId.");
        Console.WriteLine(loginJson);
        return;
    }

    Console.WriteLine($"   sessionId: {login.SessionId}");
    Console.WriteLine($"   idleTimeoutSeconds: {login.IdleTimeoutSeconds}");
    Console.WriteLine();

    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login.AccessToken);

    Console.WriteLine("2) Calling /admin immediately (expected: 200)...");
    var firstAdmin = await client.GetAsync("/admin");
    Console.WriteLine($"   status: {(int)firstAdmin.StatusCode} {firstAdmin.StatusCode}");
    Console.WriteLine($"   body: {await firstAdmin.Content.ReadAsStringAsync()}\n");

    var waitSeconds = Math.Max(1, (int)Math.Ceiling(login.IdleTimeoutSeconds) + 1);
    Console.WriteLine($"3) Waiting {waitSeconds}s to exceed idle timeout...");
    await Task.Delay(TimeSpan.FromSeconds(waitSeconds));
    Console.WriteLine();

    Console.WriteLine("4) Calling /admin again with same token (expected: 401 after idle timeout)...");
    var secondAdmin = await client.GetAsync("/admin");
    Console.WriteLine($"   status: {(int)secondAdmin.StatusCode} {secondAdmin.StatusCode}");
    Console.WriteLine($"   body: {await secondAdmin.Content.ReadAsStringAsync()}\n");

    Console.WriteLine("5) Inspecting /session/{sessionId} for revocation state...");
    var sessionState = await client.GetAsync($"/session/{login.SessionId}");
    Console.WriteLine($"   status: {(int)sessionState.StatusCode} {sessionState.StatusCode}");
    Console.WriteLine($"   body: {await sessionState.Content.ReadAsStringAsync()}\n");

    Console.WriteLine("=====================================");
    Console.WriteLine(" DEMO COMPLETE");
    Console.WriteLine("=====================================");


}

internal sealed record LoginResponse(
    string AccessToken,
    string SessionId,
    double IdleTimeoutSeconds,
    string? Note
);
