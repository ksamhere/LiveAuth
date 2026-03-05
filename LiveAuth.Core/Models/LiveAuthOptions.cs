using System.Security.Claims;

namespace LiveAuth.Core.Models
{
    public class LiveAuthOptions
    {
        public string SessionIdClaimType { get; set; } = "sid";
        public string VersionClaimType { get; set; } = "ver";
        public string RoleClaimType { get; set; } = ClaimTypes.Role;
        public bool OverrideRoleFromSession { get; set; } = true;
    }
}
