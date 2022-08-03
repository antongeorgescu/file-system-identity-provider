using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace MockWebApi.Policy
{
    // This class contains logic for determining whether MinimumAgeRequirements in authorization
    // policies are satisfied or not
    public class RoleRequiredAuthorizationHandler : AuthorizationHandler<RoleRequirement>
    {
        private readonly ILogger<RoleRequiredAuthorizationHandler> _logger;

        public RoleRequiredAuthorizationHandler(ILogger<RoleRequiredAuthorizationHandler> logger)
        {
            _logger = logger;
        }

        // Check whether a given MinimumAgeRequirement is satisfied or not for a particular context
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, RoleRequirement requirement)
        {
            // Log as a warning so that it's very clear in sample output which authorization policies 
            // (and requirements/handlers) are in use
            _logger.LogWarning("Evaluating authorization requirement for role >= {role}", requirement.Role);

            // Check the role from jwt token claims
            var rolerequired = context.User.FindFirst(c => c.Type == ClaimTypes.Role);
            if (rolerequired != null)
            {
                _logger.LogInformation("Required role authorization requirement {role} satisfied", requirement.Role);
                context.Succeed(requirement);
            }
            else
            {
                _logger.LogInformation("Current request does not satisfy the authorization requirement {role}",requirement.Role);
            }
            return Task.CompletedTask;
        }
    }
}
