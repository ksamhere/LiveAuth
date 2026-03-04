using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiveAuth.Core.Models
{
    public record SessionState
    {
        public string SessionId { get; init; } 
        public string Role { get; init; }     
        public int Version { get; init; }
        public bool IsRevoked { get; init; }
        //All of these are immutable
    }

}
