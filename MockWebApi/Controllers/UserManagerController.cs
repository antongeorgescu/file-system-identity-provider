using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MockWebApi.ActionFilters;
using MockWebApi.Log;
//using MockCbsService.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http.Controllers;

namespace MockWebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserManagerController : ControllerBase
    {
        private static readonly string[] Names = new[]
        {
            "Johnny Cecotto", "Brad Bracing", "Mary Chilly", "Emilio Cool", "Anthony Mild", "Gregory Warm", "Helmuth Balmy", "John Hot", "Adela Sweltering", "Trudy Scorching"
        };

        private readonly ILogger<UserManagerController> _logger;

        public UserManagerController(ILogger<UserManagerController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        [Route("userlist")]
        [ServiceFilter(typeof(RequestLogAttribute))]
        [ServiceFilter(typeof(CustomAuthorizationAttribute))]
        public IEnumerable<UserProfile> GetUserList()
        {
            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new UserProfile
            {
                Name = Names[rng.Next(10)],
                Age = rng.Next(25, 42)
            })
            .ToArray();
        }

    }
}
