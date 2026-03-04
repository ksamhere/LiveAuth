using LiveAuth.Core.Abstractions;
using LiveAuth.Core.Models;

public class FakeSessionStore : ISessionStateReader
{
    private static readonly Dictionary<string, SessionState> _sessions =
        new()
        {
            ["S123"] = new SessionState
            {
                SessionId = "S123",
                Version = 1,
                IsRevoked = false,
                Role = "Admin"

            },
            ["S199"] = new SessionState
            {
                SessionId = "S199",
                Version = 1,
                IsRevoked = false,
                Role = "User"
            }
        };

    public Task<SessionState?> GetSessionAsync(string sid)
        => Task.FromResult(_sessions.TryGetValue(sid, out var s) ? s : null);



}
