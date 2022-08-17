using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BareboneService.Authentication
{
    public class AzureAADTokenConformityPipe : BaseTokenConformityPipe, IConformityPipe
    {
        private IConfiguration _config;
        private IAppCache _cache;
        public AzureAADTokenConformityPipe(string authProvider, IConfiguration config, IAppCache cache) : base(authProvider)
        {
            _config = config;
            _cache = cache;
        }
        public Task ConformToken(CertificateValidatedContext context)
        {
            throw new System.NotImplementedException();
        }

        public Task ConformToken(TokenValidatedContext context)
        {
            BaseConformToken(context);

            ClaimsIdentity identity = context.Principal.Identity as ClaimsIdentity;
            if (identity != null)
            {
                // Add the access_token as a claim, as we may actually need it
                var accessToken = context.SecurityToken as JwtSecurityToken;

                // Parse the tenantOrganization custom claim as multiple claims
                ParseTenantOrganizationCustomExtension(accessToken, identity);

                //Check if the token is based on service principle
                var isServicePrinciple = accessToken.Claims.Any(c =>
                {
                    return c.Type.Equals("groups") ? c.Value == _config.GetValue<string>("Auth:AzureAd:Groups") : false;
                });
            }
            return Task.CompletedTask;
        }

        public async Task<Task> ConformTokenAsync(TokenValidatedContext context)
        {
            await BaseConformToken(context);
            if (context.Principal.Identity is ClaimsIdentity identity)
            {
                // Add the access_token as a claim, as we may actually need it
                var accessToken = context.SecurityToken as JwtSecurityToken;

                //Check if the token is based on service principle
                var isServicePrinciple = accessToken.Claims.Any(c =>
                {
                    return c.Type.Equals("groups") && c.Value == _config.GetValue<string>("Auth:AzureAd:Groups");
                });
                /* Code to fetch manifest through graph api */
                if (isServicePrinciple)
                {
                    //fetch app roles from cbs.messenger.service manifest file
                    var appRoles = await getTokenAsync();

                    foreach (var roles in appRoles)
                    {
                        identity.AddClaim(new Claim(JwtClaimNames.Roles, roles.DisplayName));
                    }
                }
            }
            return Task.CompletedTask;
        }

        private async Task<IEnumerable<AppRole>> getTokenAsync()
        {
            var json = await FromCache(_config.GetValue<string>("SystemAADAccount:ClientId"));
            if (json == null)
            {
                IConfidentialClientApplication app = ConfidentialClientApplicationBuilder
                    .Create(_config.GetValue<string>("SystemAADAccount:ClientId"))
                    .WithClientVersion("v2.0")
                    .WithTenantId(_config.GetValue<string>("SystemAADAccount:Tenant"))
                    .WithClientSecret(_config.GetValue<string>("SystemAADAccount:ClientSecret"))
                    .Build();
                ClientCredentialProvider providers = new ClientCredentialProvider(app);
                GraphServiceClient graphClient = new GraphServiceClient(providers);

                var application = await graphClient.Applications
                    .Request()
                    .GetAsync();
                var daemonApp = application.Where(x => x.AppId == _config.GetValue<string>("SystemAADAccount:ClientId")).FirstOrDefault();
                await ToCache(_config.GetValue<string>("SystemAADAccount:ClientId"), JsonConvert.SerializeObject(daemonApp.AppRoles));
                return daemonApp.AppRoles;
            }
            else
            {
                return JsonConvert.DeserializeObject<IEnumerable<AppRole>>(json);
            }
        }

        private void ParseTenantOrganizationCustomExtension(JwtSecurityToken accessToken, ClaimsIdentity identity)
        {
            var tenantOrganizationString = accessToken.Claims.FirstOrDefault(p => p.Type == "extn.tenOrg")?.Value;
            if (string.IsNullOrWhiteSpace(tenantOrganizationString))
                return;

            var tenantOrganizationCodes = tenantOrganizationString.Split(',');

            foreach (var to in tenantOrganizationCodes)
            {
                identity.AddClaim(new Claim(JwtClaimNames.TenantOrganization, to));
            }
        }

        private async Task<string> FromCache(string key)
        {
            return await _cache.GetJsonAsync<string>(string.Concat(CacheKeys.MessengerServiceAppRoles, key));
        }

        private async Task ToCache(string key, string approles)
        {
            await _cache.SetJsonAsync(string.Concat(CacheKeys.MessengerServiceAppRoles, key), approles, TimeSpan.FromSeconds(_config.GetValue<int>("Messenger:AppRolesCacheTTL")));
        }
    }
}
}
