# LiveAuth

LiveAuth is a state-aware authorization middleware for ASP.NET Core that addresses core JWT stateless limitations such as delayed revocation and stale role/permission claims.

## Why LiveAuth?
Traditional JWT access tokens are self-contained. Once issued, role revocation or session invalidation usually does **not** apply until token expiry.

LiveAuth keeps JWTs lightweight (session id + version) and validates each request against a session state store, enabling:
- immediate revocation,
- role/permission freshness,
- centralized session control.

## Install
```bash
dotnet add package LiveAuth
```

## Configure in your API
```csharp
builder.Services.AddLiveAuth(options =>
{
  options.Issuer = builder.Configuration["Jwt:Issuer"] ?? string.Empty;
  options.Audience = builder.Configuration["Jwt:Audience"] ?? string.Empty;
  options.Secret = builder.Configuration["Jwt:Secret"] ?? string.Empty;
}).AddLiveAuth(options =>
    {
        options.OverrideRoleFromSession = true;
    });
builder.Services.AddSingleton<ISessionStateStore, YourSessionStore>();

app.UseLiveAuth();
```

`appsettings.json`
```json
"Jwt": {
  "Issuer": "auth.example.com",
  "Audience": "liveauth-api",
  "Secret": "MySuperSecretKeyForHS256MustBe32Byte!"
}
```

## Sample projects
- `Samples/WebApiSample`: Uses `LiveAuth` middleware (stateful behavior).
- `Samples/WebApiSample.WithoutLiveAuth`: Baseline JWT-only API (stateless behavior).
- `Samples/LiveAuth.TestClient`: Console client to generate test tokens and exercise revocation scenarios.

### Demo idea
1. Run `WebApiSample` and call `/secure` with a valid token.
2. Revoke with `/admin/revoke/{sid}`.
3. Call `/secure` again using the same token.
   - With LiveAuth middleware: request is denied (revocation enforced).
   - Without LiveAuth middleware: request may still succeed until token expiry.


If you prefer config binding, `builder.Services.AddLiveAuth(builder.Configuration);` is also supported.
