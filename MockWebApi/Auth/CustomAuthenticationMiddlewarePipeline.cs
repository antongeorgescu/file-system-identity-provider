using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MockCbsService
{
    public class CustomAuthenticationMiddlewarePipeline
    {
        public void Configure(IApplicationBuilder app)
        {
            app.UseMyCustomAuthentication();
        }
    }
}
