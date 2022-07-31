using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MockCbsService
{
    public static class CustomAuthenticationMiddlewareExtensions
    {
        public static IApplicationBuilder UseMyCustomAuthentication(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<CustomAuthenticationMiddleware>();
        }
    }
}
