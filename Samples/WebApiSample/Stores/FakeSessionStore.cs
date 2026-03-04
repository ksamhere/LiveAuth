using LiveAuth.Core.Abstractions;
using LiveAuth.Core.Models;

public class FakeSessionStore : ISessionStateReader
{
    private static readonly Dictionary<string, SessionState> _sessions = new();

    public static void Add(SessionState session)
    {
        _sessions[session.SessionId] = session;
    }

    public static void Update(SessionState session)
    {
        _sessions[session.SessionId] = session;
    }

    public Task<SessionState?> GetSessionAsync(string sessionId)
    {
        _sessions.TryGetValue(sessionId, out var session);
        return Task.FromResult(session);
    }

    public static Task<List<SessionState>> GetAllSessionsAsync()
    {
        return Task.FromResult(_sessions.Values.ToList());
    }
}
