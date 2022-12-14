using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace TestAspNetWebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TokenManagerController : Controller
    {
        private readonly ILogger<TokenManagerController> _logger;
        private IConfiguration _configuration;

        public TokenManagerController(ILogger<TokenManagerController> logger, IConfiguration config)
        {
            _logger = logger;
            _configuration = config;
        }


        // GET: TokenGeneratorController
        [HttpGet]
        public IActionResult Index()
        {
            return Ok("Welcome to TokenManager Service!");
        }

        [HttpGet]
        [Route("generatetoken")]
        public IActionResult GenerateToken(string applicationId, string secret)
        {
            // generate local JWT token in an OpenID format, and with roles attached
            // for demo purposes all JWT attributes are kept in appsettings.json file, however for real life implementations
            // they should be kept in a secure data repository
            try
            {
                var settings = _configuration.GetSection("TokenSettingsPoc").GetChildren().Select(x => x).ToArray();
                foreach (IConfigurationSection setting in settings)
                {
                    if (setting["ApplicationId"] == applicationId)
                    {
                        var securityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secret));

                        // compare with locally stored key
                        var localSecurityKey = setting["SecurityKey"];
                        if (!BitConverter.ToString(securityKey.Key).Equals(localSecurityKey))
                            return Unauthorized();

                        var issuer = setting.GetSection("Issuer").Value;
                        var audience = setting.GetSection("Audience").Value;
                        var tokenHandler = new JwtSecurityTokenHandler();
                        var tokenDescriptor = new SecurityTokenDescriptor
                        {
                            Subject = new ClaimsIdentity(new Claim[]
                                {
                                    new Claim(ClaimTypes.NameIdentifier, setting["Application"])
                                }),
                            Expires = DateTime.UtcNow.AddMinutes(int.Parse(setting.GetSection("ExpireMinutes").Value)),
                            Issuer = issuer,
                            Audience = audience,
                            SigningCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature)
                        };

                        // check if there are any roles attached to app
                        string? strroles = setting["Roles"];
                        if (!string.IsNullOrEmpty(strroles))
                            foreach (string role in strroles.Split(','))
                                tokenDescriptor.Subject.AddClaim(new Claim(ClaimTypes.Role, role));

                        var token = tokenHandler.CreateToken(tokenDescriptor);
                        return Ok(tokenHandler.WriteToken(token));
                    }
                }

                return Unauthorized();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
