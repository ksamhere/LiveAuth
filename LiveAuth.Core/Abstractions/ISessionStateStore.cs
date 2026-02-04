using LiveAuth.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiveAuth.Core.Abstractions
{
    public interface ISessionStateStore
    {
        Task<SessionState?> GetAsync(string sid);
    }

    public class InMemorySessionStateStore : ISessionStateStore
    {
        private static readonly Dictionary<string, SessionState> _db = new()
        {
            ["S123"] = new SessionState
            {
                SessionId = "S123",
                Version = 1,
                Revoked = false,
                TenantId = "tenant1",
                Roles = new[] { "Admin" },
                Scopes = new[] { "read" }
            },
            ["S999"] = new SessionState
            {
                SessionId = "S999",
                Version = 1,
                Revoked = true,
                TenantId = "tenant1",
                Roles = new[] { "User" },
                Scopes = new[] { "read" }
            }
        };

        public Task<SessionState?> GetAsync(string sid)
            => Task.FromResult(_db.TryGetValue(sid, out var s) ? s : null);
    }
}
