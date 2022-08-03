using Microsoft.AspNetCore.Authorization;

namespace MockWebApi.Policy
{
    // This attribute derives from the [Authorize] attribute, adding 
    // the ability for a user to specify a 'RequiredRole' paratmer.
    public class ContributorRoleAuthorizeAttribute : AuthorizeAttribute
    {
        const string POLICY_PREFIX = "RoleRequired";

        public ContributorRoleAuthorizeAttribute(string role) => Role = role;

        // Get or set the Age property by manipulating the underlying Policy property
        public string Role
        {
            get
            {
                return Policy.Substring(POLICY_PREFIX.Length);
            }
            set
            {
                Policy = $"{POLICY_PREFIX}{value.ToString()}";
            }
        }
    }
}