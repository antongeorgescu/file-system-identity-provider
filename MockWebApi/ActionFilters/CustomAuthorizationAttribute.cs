using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;

namespace MockWebApi.ActionFilters
{
    public class CustomAuthorizationAttribute : IActionFilter
    {
        public bool AllowMultiple => throw new NotImplementedException();

        public void OnActionExecuting(ActionExecutingContext filterContext)
        {
            string bearer_token = filterContext.HttpContext.Request.Headers["Bearer"];

            // Example: https://sts.windows.net/fa15d692-e9c7-4460-a743-29f29522229/
            const string AAD_TOKEN_V1 = @"https://sts.windows.net";

            // Example: http://filesysidprovider.com
            const string SLP_TOKEN_V1 = @"http://filesysidprovider.com";

            // Example: https://login.microsoftonline.com/72f988bf-86f1-41af-91ab-2d7cd011db47/v2.0
            const string AAD_TOKEN_V2 = @"https://login.microsoftonline.com";

            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtSecurityToken = handler.ReadJwtToken(bearer_token);

                var issuer = jwtSecurityToken.Claims.First(claim => claim.Type == "iss").Value;
                var nameid = string.Empty;
                var strroles = string.Empty;
                var striss = "unknown";
                IEnumerable<Claim> roles;

                // extract roles from 3 different types of tokens
                if (issuer.StartsWith(AAD_TOKEN_V1))
                {
                    striss = "azure ad v1.0";
                    nameid = jwtSecurityToken.Claims.First(claim => claim.Type == "name").Value;
                    strroles = jwtSecurityToken.Claims.First(claim => claim.Type == "roles").Value;
                }
                else if (issuer.StartsWith(AAD_TOKEN_V2))
                {
                    striss = "azure ad v2.0";
                    nameid = jwtSecurityToken.Claims.First(claim => claim.Type == "name").Value;
                    strroles = jwtSecurityToken.Claims.First(claim => claim.Type == "roles").Value;
                }
                else if (issuer.StartsWith(SLP_TOKEN_V1))
                {
                    striss = "sl platform v1.0";
                    var lstroles = new List<string>();
                    nameid = jwtSecurityToken.Claims.First(claim => claim.Type == "nameid").Value;
                    roles = jwtSecurityToken.Claims.Where(claim => claim.Type == "role");
                    foreach (Claim role in roles)
                        lstroles.Add(role.Value);
                    strroles = string.Join(",", lstroles);
                }
                else
                {
                    //filterContext.HttpContext.Response.StatusCode = 401;
                    // return 'Unauthorized' for any exception in authorization action filter
                    filterContext.Result = new UnauthorizedObjectResult("Unauthorized request");
                    return;
                }
                
                // check if any role is assigned to respective path
                var endpointList = FileReader.LoadServiceRolesJson();
                var endpointInRole = endpointList.Select(x => x).Where(x => ((x.endpointPath == filterContext.HttpContext.Request.Path.Value) && (strroles.Contains(x.requiredRole))));

                // got to next pipeline step (consume endpoint) if at least one required role has been identified;
                // otherwise return response with 'Unauthorized'
                if (endpointInRole.Count() == 0)
                {
                    HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
                    response.RequestMessage.CreateResponse(HttpStatusCode.Unauthorized);
                }
            }
            catch (Exception ex)
            {
                // return 'Unauthorized' for any exception in authorization action filter
                filterContext.Result = new UnauthorizedObjectResult("Unauthorized request");
                
            }
        }

        public void OnActionExecuted(ActionExecutedContext filterContext)
        {
            
        }

        public Task<HttpResponseMessage> ExecuteActionFilterAsync(HttpActionContext actionContext, CancellationToken cancellationToken, Func<Task<HttpResponseMessage>> continuation)
        {
            throw new NotImplementedException();
        }
    }
}
