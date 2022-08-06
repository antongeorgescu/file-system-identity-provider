﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using MockWebApi.AuthorizationStore;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace MockWebApi.ActionFilters
{

    public class EndpointAuthorizeAttribute : ActionFilterAttribute
    {
        private IAuthorizationService AuthService { get; set; }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            bool skip = false;
            var controllerActionDescriptor = context.ActionDescriptor as ControllerActionDescriptor;
            if (controllerActionDescriptor != null)
            {
                var routeAttributeObj = controllerActionDescriptor.MethodInfo.GetCustomAttributes(typeof(AllowAnonymousAttribute), inherit: true).FirstOrDefault();
                skip = routeAttributeObj != null;
            }

            if (!skip)
            {
                // check if any role is assigned to respective path
                var endpointList = ServiceRoles.LoadServiceRolesJson();
                var bearer_token = context.HttpContext.Request.Headers["Bearer"];

                var handler = new JwtSecurityTokenHandler();
                var jwtSecurityToken = handler.ReadJwtToken(bearer_token);
                var strroles = jwtSecurityToken.Claims.First(claim => claim.Type == "roles").Value;

                var endpointInRole = endpointList.Select(x => x).Where(x => ((x.endpointPath == context.HttpContext.Request.Path.Value) && (strroles.Contains(x.requiredRole))));

                // got to next pipeline step (consume endpoint) if at least one required role has been identified;
                // otherwise return response with 'Unauthorized'
                if (endpointInRole.Count() > 0)
                    await next();
                else
                    //context.HttpContext.Response.StatusCode = 401;
                    context.Result = new ForbidResult();
            }
            else await next();
        }


    }
}

