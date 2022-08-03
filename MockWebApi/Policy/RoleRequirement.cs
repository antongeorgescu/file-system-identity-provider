using Microsoft.AspNetCore.Authorization;

namespace MockWebApi.Policy
{
    public class RoleRequirement : IAuthorizationRequirement
    {
        public string Role { get; private set; }

        public RoleRequirement(string role) { Role = role; }
    }
}