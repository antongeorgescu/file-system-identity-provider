using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace TokenGeneratorService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TokenManagerController : ControllerBase
    {
        private readonly ILogger<TokenManagerController> _logger;
		private IConfiguration _configuration;

        public TokenManagerController(ILogger<TokenManagerController> logger, IConfiguration config)
        {
            _logger = logger;
			_configuration = config;
		}

        [HttpGet]
        public string Get()
        {
            return "Welcome to TokenManagerService!";
        }

		[HttpGet]
		[Route("generatetoken")]
		public string GetGenerateToken(string app, string applicationId, string secret)
		{
			//var secret = "asdv234234^&%&^%&^hjsdfb2%%%";

			var settings = _configuration
								.GetSection("TokenSettings")
								.GetChildren()
								.Select(x => x)
								.ToArray();
			foreach (IConfigurationSection setting in settings) {
				if (setting["Application"] == app)
                {
					var securityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secret));

					// compare with locally stored key
					var localSecurityKey = setting["SecurityKey"];
					if (!BitConverter.ToString(securityKey.Key).Equals(localSecurityKey))
						return "401";

					var issuer = setting.GetSection("Issuer").Value;
					var audience = setting.GetSection("Audience").Value;
					var tokenHandler = new JwtSecurityTokenHandler();
					var tokenDescriptor = new SecurityTokenDescriptor
					{
						Subject = new ClaimsIdentity(new Claim[]
							{
								new Claim(ClaimTypes.NameIdentifier, applicationId)
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
					return tokenHandler.WriteToken(token);
				}
            }

			return "401";


		}
	}
}
