using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace MockCbsService
{
    public class CustomAuthenticationMiddleware
    {
        private readonly RequestDelegate next;

        public CustomAuthenticationMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            string bearer_token = context.Request.Headers["Bearer"];

            //    if (authoriztionHeader != null && authoriztionHeader.StartsWith("Basic"))
            //    {
            //        var encodedUsernamePassword = authoriztionHeader.Substring("Basic ".Length).Trim();
            //        var encoding = Encoding.GetEncoding("iso-8859-1");
            //        var usernamePassword = encoding.GetString(Convert.FromBase64String(encodedUsernamePassword));
            //        var seperatorIndex = usernamePassword.IndexOf(':');
            //        var username = usernamePassword.Substring(0, seperatorIndex);
            //        var password = usernamePassword.Substring(seperatorIndex + 1);

            //        /*if (GetUsers.GetDatabaseUsers.CheckCredentials(username, password))
            //        {
            //            // your additional logic here...

            //            await this.next.Invoke(context);
            //        }
            //        else
            //        {
            //            context.Response.StatusCode = 401;
            //        }*/
            //    }
            //    else
            //    {
            //        context.Response.StatusCode = 401;
            //    }
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
                var endpointList = LoadServiceRolesJson();
                var endpointInRole = endpointList.Select(x => x).Where(x => ((x.endpointPath == context.Request.Path.Value) && (strroles.Contains(x.requiredRole))));
                if (endpointInRole.Count()>0)
                    await this.next.Invoke(context);
                else
                    context.Response.StatusCode = 401;
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = 401;
            }
        }

        public List<EndpointRole> LoadServiceRolesJson()
        {
            var entries = new List<EndpointRole>();
            using (StreamReader r = new StreamReader(@"auth\serviceroles.json"))
            {
                string jsonfile = r.ReadToEnd();
                
                var jsonstr = JsonConvert.DeserializeObject(jsonfile);
                foreach (dynamic entry in ((dynamic)jsonstr).endpoints)
                {
                    var endpointRole = new EndpointRole();
                    endpointRole.endpointPath = entry.endpoint.Value;
                    endpointRole.requiredRole = entry.roles.Value;
                    entries.Add(endpointRole);
                }
                    
            }
            return entries;
        }
    }

    public class EndpointRole
    {
        public string endpointPath;
        public string requiredRole;
    }
}
