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

namespace TokenDecoder
{
    class Program
    {
        static void Main(string[] args)
        {
            string bearer_token = "";
            string endpoint_path = "/loanmanager/loanlist";

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
                    Console.WriteLine("Unauthorized");
                    Console.ReadLine();
                    return;
                }

                // check if any role is assigned to respective path
                var endpointList = LoadServiceRolesJson();
                var endpointInRole = endpointList.Select(x => x).Where(x => ((x.endpointPath == endpoint_path) && (strroles.Contains(x.requiredRole))));

                // got to next pipeline step (consume endpoint) if at least one required role has been identified;
                // otherwise return response with 'Unauthorized'
                if (endpointInRole.Count() > 0)
                    Console.WriteLine("Authorized, go process endpoint"); 
                else
                    Console.WriteLine("Unauthorized");
                Console.ReadLine();
                return;
            }
            catch (Exception ex)
            {
                // return 'Unauthorized' for any exception in authorization middleware
                Console.WriteLine("Unauthorized");
                Console.ReadLine();
                return;
            }
        }

        public static List<EndpointRole> LoadServiceRolesJson()
        {
            var entries = new List<EndpointRole>();
            using (StreamReader r = new StreamReader(@"auth\service.access.roles.json"))
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
