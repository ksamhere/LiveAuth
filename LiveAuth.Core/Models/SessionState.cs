using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiveAuth.Core.Models
{
    public class SessionState
    {
        public string SessionId { get; set; }
        public string UserId { get; set; }
        public string TenantId { get; set; }
        public int Version { get; set; }
        public bool Revoked { get; set; }
        public string[] Roles { get; set; } = [];
        public string[] Scopes { get; set; } = [];
    }

}
