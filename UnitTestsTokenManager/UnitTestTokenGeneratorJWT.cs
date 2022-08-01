using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Claims;
using TokenGenerator.Managers.Interfaces;
using Xunit;
using Xunit.Abstractions;
using TokenGenerator.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace UnitTestsTokenManager
{
    public class UnitTestTokenGeneratorJWT
    {
        private readonly ITestOutputHelper output;
        private readonly ITokenManager _tokenManager;
        private readonly IOptions<TokenGenerator.Models.TokenSettings> _settings;

        public UnitTestTokenGeneratorJWT(ITestOutputHelper output)
        {
            this.output = output;
            _settings = (IOptions<TokenSettings>)(new TokenSettings());
            _settings.Value.Audience = "aud_hafshasgahgsahgsa";
            _settings.Value.Issuer = "iss_hgdahgdajshgdashdgja";
            _settings.Value.ExpireMinute = 450;
            _settings.Value.SigningKey = "234567890";

            _tokenManager = new TokenGenerator.Managers.TokenManager(_settings);
        }

        [Fact]
        public void GenerateToken()
        {
            try
            {
                var token = _tokenManager.GenerateToken(new Dictionary<string, object>
                {
                    {
                        "userId", Guid.NewGuid()
                    },
                    {
                        "userEmail", "test@gmail.com"
                    },
                    {
                        "nameAndSurname", "Test Test"
                    }
                });
                output.WriteLine(token);
                Assert.True(token.Length > 0);
            }
            catch(Exception ex)
            {
                output.WriteLine(ex.Message);
                Assert.True(string.IsNullOrEmpty(ex.Message));
            }
        }

        
    }
}