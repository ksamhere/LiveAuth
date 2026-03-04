using LiveAuth.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiveAuth.Core.Abstractions
{
    public interface ISessionStateReader
    {
        Task<SessionState?> GetSessionAsync(string sid);        
    }

    
}
