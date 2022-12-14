using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BareboneServicePolicy.Authentication
{
    public class PrincipalConformity
    {
        private const string TokenPrefix = "Bearer ";
        public static Task ConformToken(TokenValidatedContext context)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtSecurityToken = handler.ReadJwtToken(context.HttpContext.Request.Headers["Authorization"]).ToString().Substring(TokenPrefix.Length)); ;

            var issuer = jwtSecurityToken.Claims.First(claim => claim.Type == UserSettings.Issuer).Value;
            var audience = jwtSecurityToken.Claims.First(claim => claim.Type == UserSettings.Audience).Value;
            var useremail = jwtSecurityToken.Claims.First(claim => claim.Type == UserSettings.UserEmail).Value;
            var name = jwtSecurityToken.Claims.First(claim => claim.Type == UserSettings.Name).Value;
            var nameid = jwtSecurityToken.Claims.First(claim => claim.Type == UserSettings.NameIdentifier).Value;
            var strroles = string.Empty;
            
            var identity = new ClaimsIdentity(new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, nameid),
                    new Claim(ClaimTypes.Name, name),
                    new Claim(ClaimTypes.Email, useremail)
                }, "Custom");

            context.HttpContext.User = new ClaimsPrincipal(identity);

            return Task.CompletedTask;
        }

        public static Task ConformToken(HttpContext context)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtSecurityToken = handler.ReadJwtToken(context.Request.Headers["Bearer"]);

            var issuer = jwtSecurityToken.Claims.First(claim => claim.Type == UserSettings.Issuer).Value;
            var audience = jwtSecurityToken.Claims.First(claim => claim.Type == UserSettings.Audience).Value;
            var useremail = jwtSecurityToken.Claims.First(claim => claim.Type == UserSettings.UserEmail).Value;
            var name = jwtSecurityToken.Claims.First(claim => claim.Type == UserSettings.Name).Value;
            var nameid = jwtSecurityToken.Claims.First(claim => claim.Type == UserSettings.NameIdentifier).Value;
            var strroles = string.Empty;

            var identity = new ClaimsIdentity(new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, nameid),
                    new Claim(ClaimTypes.Name, name),
                    new Claim(ClaimTypes.Email, useremail)
                }, "Custom");

            context.User = new ClaimsPrincipal(identity);

            return Task.CompletedTask;
        }
    }
}
