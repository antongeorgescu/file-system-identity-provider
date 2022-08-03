using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MockWebApi.Middleware
{
    public class CustomAuthorizationMiddlewarePipeline
    {
        public void Configure(IApplicationBuilder app)
        {
            app.UseCustomAuthorization();
        }
    }
}
