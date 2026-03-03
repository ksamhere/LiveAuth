using LiveAuth.Core.Abstractions;
using LiveAuth.Core.Models;

public class FakeSessionStore : ISessionStateStore
{
    private static readonly Dictionary<string, SessionState> _sessions =
        new()
        {
            ["S123"] = new SessionState
            {
                SessionId = "S123",
                UserId = "user1",
                TenantId = "tenant1",
                Version = 1,
                IsRevoked = false,
                Roles = ["Admin"],
                Scopes = ["orders.read"]
            },
            ["S199"] = new SessionState
            {
                SessionId = "S199",
                UserId = "user1",
                TenantId = "tenant1",
                Version = 1,
                IsRevoked = false,
                Roles = ["User"],
                Scopes = ["orders.read"]
            }
        };

    public Task<SessionState?> GetSessionAsync(string sid)
        => Task.FromResult(_sessions.TryGetValue(sid, out var s) ? s : null);

    public Task RevokeSessionAsync(string sid)
    {
        if (_sessions.TryGetValue(sid, out var session))
        {
            session.IsRevoked = true;
        }
        return Task.CompletedTask;
    }

    public Task SetSessionAsync(SessionState session)
    {
        _sessions[session.SessionId] = session;
        return Task.CompletedTask;
    }

    
}
