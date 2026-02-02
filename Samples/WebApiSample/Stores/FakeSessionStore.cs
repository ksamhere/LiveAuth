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
                Revoked = false,
                Roles = ["Admin"],
                Scopes = ["orders.read"]
            }
        };

    public Task<SessionState?> GetAsync(string sid)
        => Task.FromResult(_sessions.TryGetValue(sid, out var s) ? s : null);
}
