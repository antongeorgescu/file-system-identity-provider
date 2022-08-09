using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace BareboneServiceActionFilter.Auth
{
    public class ServiceRoles
    {
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
