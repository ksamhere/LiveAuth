using LiveAuth.Core.Abstractions;
using LiveAuth.Core.Models;

public class FakeSessionStore : ISessionStateStore
{
    private static readonly Dictionary<string, SessionState> Sessions =
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
            }
        };

    public Task<SessionState?> GetSessionAsync(string sid)
        => Task.FromResult(Sessions.TryGetValue(sid, out var session) ? session : null);

    public Task RevokeSessionAsync(string sid)
    {
        if (Sessions.TryGetValue(sid, out var session))
        {
            session.IsRevoked = true;
        }

        return Task.CompletedTask;
    }

    public Task SetSessionAsync(SessionState session)
    {
        Sessions[session.SessionId] = session;
        return Task.CompletedTask;
    }
}
