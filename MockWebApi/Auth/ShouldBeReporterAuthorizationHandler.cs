using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MockWebApi.Auth
{
    public class ShouldBeReporterAuthorizationHandler : AuthorizationHandler<ShouldBeReporterRequirement>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            ShouldBeReporterRequirement requirement)
        {
            if (!context.User.HasClaim(x => x.Type == ClaimTypes.Email))
                return Task.CompletedTask;

            var emailAddress = context.User.Claims.FirstOrDefault(
                    x => x.Type == ClaimTypes.Email).Value;

            // check if the datastore contains the emailAddress
            // of the incoming user context (from the token)
            //if (ReaderStore.Readers.Any(
            //        x => x.EmailAddress == emailAddress))
            //{
            //    context.Succeed(requirement);
            //}

            context.Succeed(requirement);
            return Task.CompletedTask;


            

        }

        
    }

    public class ShouldBeReporterRequirement : IAuthorizationRequirement
    {
        public ShouldBeReporterRequirement()
        {
        }
    }
}
