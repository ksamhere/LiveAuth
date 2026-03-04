# LiveAuth

LiveAuth is a production-ready ASP.NET Core extension that adds dynamic session validation and role control on top of JWT authentication.

JWT tokens are immutable by design. Once issued, they cannot:

- Be revoked immediately  
- Reflect real-time role changes  
- Be invalidated centrally  
- Support forced logout  

LiveAuth solves this by validating every request against a central session store while preserving standard JWT authentication.

---

## Key Features

- Validate JWT against a central session store  
- Immediate session revocation  
- Version-based token invalidation  
- Optional dynamic role override  
- Works with existing ASP.NET Core JWT authentication  
- No scheme conflicts  
- No custom authentication handler required  

---
## Architecture

```
Client Request
   ↓
ASP.NET Core JWT Authentication
   ↓
LiveAuth (OnTokenValidated Hook)
   - Validate session exists
   - Check IsRevoked flag
   - Compare token version vs session version
   - Optionally override role
   ↓
Authorization
   ↓
Controller

```
---

## Install
```bash
dotnet add package LiveAuth
```
## Setup in ASP.NET Core WebAPI

### Configure JWT Authentication (Host Responsibility)
```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme) .AddJwtBearer(options =>
{
  options.TokenValidationParameters = new TokenValidationParameters
  {
    ValidateIssuer = true,
    ValidateAudience = true,
    ValidateLifetime = true,
    ValidateIssuerSigningKey = true,
    IssuerSigningKey = new SymmetricSecurityKey( Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]))
  };
});

```
### Add LiveAuth
```csharp
builder.Services.AddLiveAuth(options =>
{
  options.OverrideRoleFromSession = true;
});
```
That’s it. No additional middleware registration required when using the OnTokenValidated integration pattern.

### Update appsetting.json with UseLiveAuth
`appsettings.json`
```json
"Jwt": {
  "Issuer": "auth.example.com",
  "Audience": "liveauth-api",
  "Secret": "MySuperSecretKeyForHS256MustBe32Byte!"
},
"UseLiveAuth": true
```

## Session Model (Version-Based Invalidation)
  LiveAuth depends on a session store.
  ### Recommended Session Structure
  ```csharp
public record SessionState
{
  public string SessionId { get; init; } = string.Empty;
  public string Role { get; init; } = "User";
  public int Version { get; init; } = 1;
  public bool IsRevoked { get; init; } = false;
}
```
### Why Use a Record?
- Supports with expression updates
- Safer immutable state
- Better concurrency handling

### Issuing JWT with Session Version
When generating a JWT:
```csharp
var claims = new[]
{
    new Claim("sub", "admin"),
    new Claim("sid", session.SessionId),
    new Claim("ver", session.Version.ToString()),
    new Claim(ClaimTypes.Role, session.Role)
};
```
### Claims Explained
- sid → Links JWT to session
- ver → Version control mechanism
- role → Initial role (can be overridden)

## How Version-Based Invalidation Works
LiveAuth validator compares:

Token ver claim

vs

SessionState.Version

### Validator Logic
```csharp
if (tokenVer < session.Version)
{
    context.Fail("Token version outdated");
    return;
}
```
If session version increases, all older tokens immediately fail.
### Forcing Logout (Version Upgrade)

To invalidate existing tokens:
```csharp
var updatedSession = session with
{
    Version = session.Version + 1
};

await sessionStore.UpdateAsync(updatedSession);
```
#### Result
- Old token: ver = 1
- Session now: Version = 2
- Next request → Unauthorized
No need to wait for JWT expiration.

### Revoking a Session
```csharp
var revokedSession = session with
{
    IsRevoked = true
};

await sessionStore.UpdateAsync(revokedSession);
```
### Validator
```csharp
if (session.IsRevoked)
{
    context.Fail("Session revoked");
}
```

Immediate logout.

---
### Optional Role Override
If enabled:
```csharp
options.OverrideRoleFromSession = true;
```
LiveAuth replaces the role claim from the session store:
```csharp
identity.RemoveClaim(existingRoleClaim);
identity.AddClaim(new Claim(ClaimTypes.Role, session.Role));
```
This Allows
 - Downgrading Admin → User instantly
 - Promoting User → Admin instantly
 - Real-time RBAC enforcement



## Sample projects
- `Samples/WebApiSample`: Uses `LiveAuth` middleware (stateful behavior).
- `Samples/LiveAuth.TestClient`: Console client to generate test tokens and exercise revocation scenarios.

### Console Demo Scenario

1. Login as Admin
2. Call /admin → 200 OK
3. Upgrade session version
4. Call /admin again → 401 Unauthorized
5. Login again → 200 OK


### Demo idea
1. Run `WebApiSample` and then the console app which calls `/login/admin` with a valid token.
2. Revoke with `/admin/revoke/{sid}` or force upgrade to change version of the session
3. Call `/admin` again using the same token.
   - With LiveAuth middleware: request is denied (revocation enforced).
   - Without LiveAuth middleware: request may still succeed until token expiry.


### Production Recommendations
  - Use Redis or SQL as distributed session store
  - Keep session state immutable (record type)
  - Do not register JWT inside LiveAuth
  - Do not bypass HTTPS validation in production
  - Keep JWT lifetime reasonable even with versioning

### What LiveAuth Does NOT Do
  - Does not issue JWT
  - Does not replace authentication
  - Does not manage cookies
  - Does not store sessions internally
  - Does not register authentication schemes

It strictly extends existing JWT authentication.
