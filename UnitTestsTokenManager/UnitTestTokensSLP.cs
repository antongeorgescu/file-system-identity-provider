using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Claims;
using Xunit;
using Xunit.Abstractions;

namespace UnitTestsTokenManager
{
    public class UnitTestTokensSLP
    {
        private readonly ITestOutputHelper output;

        // Example: https://sts.windows.net/fa15d692-e9c7-4460-a743-29f29522229/
        const string AAD_TOKEN_V1 = @"https://sts.windows.net";
        // Example: http://filesysidprovider.com
        const string SLP_TOKEN_V1 = @"http://filesysidprovider.com";
        // Example: https://login.microsoftonline.com/72f988bf-86f1-41af-91ab-2d7cd011db47/v2.0
        const string AAD_TOKEN_V2 = @"https://login.microsoftonline.com";

        public UnitTestTokensSLP(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void TokenDecode_SLP_V1()
        {
            string bearer_token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1laWQiOiJGREEiLCJyb2xlIjpbIkZEQUNvbnRyaWJ1dG9yIiwiRkRBUmVwb3J0ZXIiXSwibmJmIjoxNjU5MjI2OTg2LCJleHAiOjE2NTkyMzQxODYsImlhdCI6MTY1OTIyNjk4NiwiaXNzIjoiaHR0cDovL2ZpbGVzeXNpZHByb3ZpZGVyLmNvbSIsImF1ZCI6Imh0dHA6Ly9maWxlc3lzaWRwcm92aWRlci5mZGEuY29tIn0.zVwuierZ1l9VZ6zClNC4BAMXzyF91lY-u94c2pQObys";
            string endpoint_path = "/loanmanager/loanlist";

            // Example: http://filesysidprovider.com
            const string SLP_TOKEN_V1 = @"http://filesysidprovider.com";

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
                    throw new Exception("Not accepted JWT type");
                }

                // check if any role is assigned to respective path
                var endpointList = LoadServiceRolesJson();
                var endpointInRole = endpointList.Select(x => x).Where(x => ((x.endpointPath == endpoint_path) && (strroles.Contains(x.requiredRole))));

                Assert.True(endpointInRole.Count() > 0, "Authorized, go process endpoint");

            }
            catch (Exception ex)
            {
                // return 'Unauthorized' for any exception in authorization middleware
                output.WriteLine("Unauthorized");
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
}
