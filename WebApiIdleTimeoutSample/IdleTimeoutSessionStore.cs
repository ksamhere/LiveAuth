using System.Collections.Concurrent;
using LiveAuth.Core.Abstractions;
using LiveAuth.Core.Models;

public sealed class IdleTimeoutSessionStore : ISessionStateReader
{
    private readonly ConcurrentDictionary<string, SessionState> _sessions = new();
    private readonly ConcurrentDictionary<string, DateTimeOffset> _lastActivity = new();

    public TimeSpan IdleTimeout { get; }

    public IdleTimeoutSessionStore(IConfiguration configuration)
    {
        var seconds = configuration.GetValue<int?>("LiveAuthIdleTimeoutSeconds") ?? 10;
        IdleTimeout = TimeSpan.FromSeconds(seconds);
    }

    public void Add(SessionState session)
    {
        _sessions[session.SessionId] = session;
        _lastActivity[session.SessionId] = DateTimeOffset.UtcNow;
    }

    public bool Revoke(string sessionId)
    {
        if (!_sessions.TryGetValue(sessionId, out var session))
        {
            return false;
        }

        _sessions[sessionId] = session with { IsRevoked = true };
        return true;
    }

    public Task<SessionState?> GetSessionAsync(string sessionId)
    {
        if (!_sessions.TryGetValue(sessionId, out var session))
        {
            return Task.FromResult<SessionState?>(null);
        }

        if (session.IsRevoked)
        {
            return Task.FromResult<SessionState?>(session);
        }

        var now = DateTimeOffset.UtcNow;
        var lastSeen = _lastActivity.GetOrAdd(sessionId, now);

        if (now - lastSeen > IdleTimeout)
        {
            var revoked = session with { IsRevoked = true };
            _sessions[sessionId] = revoked;
            return Task.FromResult<SessionState?>(revoked);
        }

        _lastActivity[sessionId] = now;
        return Task.FromResult<SessionState?>(session);
    }

    public object? GetDebugState(string sessionId)
    {
        if (!_sessions.TryGetValue(sessionId, out var session))
        {
            return null;
        }

        _lastActivity.TryGetValue(sessionId, out var lastActivity);

        return new
        {
            session.SessionId,
            session.Role,
            session.Version,
            session.IsRevoked,
            lastActivityUtc = lastActivity,
            idleTimeoutSeconds = IdleTimeout.TotalSeconds,
            nowUtc = DateTimeOffset.UtcNow
        };
    }
}