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
            try
            {
                var settings = _configuration.GetSection("TokenSettings").GetChildren().Select(x => x).ToArray();
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

        /*[HttpGet]
        [Route("decodetoken")]
        public IActionResult DecodeToken(string token)
        {
            // Example: https://sts.windows.net/fa15d692-e9c7-4460-a743-29f29522229/
            const string AAD_TOKEN_V1 = @"https://sts.windows.net";

            // Example: http://filesysidprovider.com
            const string SLP_TOKEN_V1 = @"http://filesysidprovider.com";

            // Example: https://login.microsoftonline.com/72f988bf-86f1-41af-91ab-2d7cd011db47/v2.0
            const string AAD_TOKEN_V2 = @"https://login.microsoftonline.com";

            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtSecurityToken = handler.ReadJwtToken(token);

                var issuer = jwtSecurityToken.Claims.First(claim => claim.Type == "iss").Value;
                var nameid = string.Empty;
                var strroles = string.Empty;
                var striss = "unknown";
                IEnumerable<Claim> roles;

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
                    return Unauthorized();

                return Ok($"openid_token:{striss},name_id:{nameid},roles:{strroles}");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }*/

    }
}
