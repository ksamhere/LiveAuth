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
- Samples/WebApiIdleTimeoutSample`: Web API sample that wires `AddAuthentication(...).AddJwtBearer(...).AddLiveAuth(...)` and demonstrates idle-timeout logout by revoking inactive sessions from the session store.
- `Samples/LiveAuth.TestClient`: Console client to generate test tokens and exercise revocation scenarios.

### Demo idea
1. Run `WebApiSample` and call `/secure` with a valid token.
2. Revoke with `/admin/revoke/{sid}`.
3. Call `/secure` again using the same token.
   - With LiveAuth middleware: request is denied (revocation enforced).
   - Without LiveAuth middleware: request may still succeed until token expiry.

### Idle Timeout Demo (API + Console Client)
1. Run `Samples/WebApiIdleTimeoutSample` (default `http://localhost:5000`).
2. Run `Samples/TestClient` (or pass a base URL as the first argument).
3. Observe the flow:
   - login succeeds
   - immediate `/admin` call succeeds
   - client waits beyond idle timeout
   - second `/admin` call returns `401 Unauthorized`
   - `/session/{sid}` shows `isRevoked = true`

### Production Recommendations
  - Use Redis or SQL as distributed session store
  - Keep session state immutable (record type)
  - Do not register JWT inside LiveAuth
  - Do not bypass HTTPS validation in production
  - Keep JWT lifetime reasonable even with versioning
