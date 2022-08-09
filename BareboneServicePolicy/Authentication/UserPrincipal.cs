using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BareboneServicePolicy.Authentication
{
    public static class UserSettings
    {
        public const string Issuer = "iss";
        public const string Audience = "aud";
        public const string Name = "Name";
        public const string NameIdentifier = "NameId";
        public const string UserEmail = "Email";
    }

    class JwtUserPrincipalUserFilter : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtSecurityToken = handler.ReadJwtToken(context.HttpContext.Request.Headers["Bearer"]);

            var issuer = jwtSecurityToken.Claims.First(claim => claim.Type == UserSettings.Issuer).Value;
            var audience = jwtSecurityToken.Claims.First(claim => claim.Type == UserSettings.Audience).Value;
            var useremail = jwtSecurityToken.Claims.First(claim => claim.Type == UserSettings.UserEmail).Value;
            var name = jwtSecurityToken.Claims.First(claim => claim.Type == UserSettings.Name).Value;
            var nameid = jwtSecurityToken.Claims.First(claim => claim.Type == UserSettings.NameIdentifier).Value;
            var strroles = string.Empty;

            var identity = new ClaimsIdentity(new List<Claim>
            {
                //new Claim(ClaimTypes.NameIdentifier, UserSettings.UserId),
                //new Claim(ClaimTypes.Name, UserSettings.Name),
                //new Claim(ClaimTypes.Email, UserSettings.UserEmail),
                //new Claim(ClaimTypes.Role, "Admin")
                new Claim(ClaimTypes.NameIdentifier, nameid),
                new Claim(ClaimTypes.Name, name),
                new Claim(ClaimTypes.Email, useremail)
            },"Custom");

            context.HttpContext.User = new ClaimsPrincipal(identity);
            await next();
        }
    }
}
