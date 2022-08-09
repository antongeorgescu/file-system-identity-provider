using Microsoft.AspNetCore.Http;
using MockCbsService.Auth;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace BareboneServiceMiddleware
{
    public class CustomAuthorizationMiddleware
    {
        private readonly RequestDelegate next;

        public CustomAuthorizationMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            string bearer_token = context.Request.Headers["Bearer"];
                        
            // Example: https://sts.windows.net/fa15d692-e9c7-4460-a743-29f29522229/
            const string AAD_TOKEN_V1 = @"https://sts.windows.net";

            // Example: http://filesysidprovider.com
            const string SLP_TOKEN_V1 = @"http://filesysidprovider.com";

            // Example: https://login.microsoftonline.com/72f988bf-86f1-41af-91ab-2d7cd011db47/v2.0
            const string AAD_TOKEN_V2 = @"https://login.microsoftonline.com";

            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtSecurityToken = handler.ReadJwtToken(context.Request.Headers["Bearer"]);

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
                    context.Response.StatusCode = 401;

                // check if any role is assigned to respective path
                var endpointList = ServiceRoles.LoadServiceRolesJson();
                var endpointInRole = endpointList.Select(x => x).Where(x => ((x.endpointPath == context.Request.Path.Value) && (strroles.Contains(x.requiredRole))));

                // got to next pipeline step (consume endpoint) if at least one required role has been identified;
                // otherwise return response with 'Unauthorized'
                if (endpointInRole.Count()>0)
                    await this.next.Invoke(context);
                else
                    context.Response.StatusCode = 401;
            }
            catch (Exception ex)
            {
                // return 'Unauthorized' for any exception in authorization middleware
                context.Response.StatusCode = 401;
            }
        }

        
    }

    public class EndpointRole
    {
        public string endpointPath;
        public string requiredRole;
    }
}
