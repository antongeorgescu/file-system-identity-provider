using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BareboneServicePolicy.Controllers
{
    [ApiController]
    [Route("[controller]")]
    //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    //[Authorize(Policy = "HasProtectedAccess")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;
        private IAuthorizationService _authorization;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IAuthorizationService authorizationService)
        {
            _logger = logger;
            _authorization = authorizationService;
        }

        [HttpGet]
        [Route("dailyforecast")]
        //[Authorize(Policy = "HasProtectedAccess")]
        //public IEnumerable<DailyForecast> GetDaily()
        public async Task<object> GetDaily()
        {
            var allowed = await _authorization.AuthorizeAsync(User, "HasProtectedAccess");
            if (!allowed.Succeeded)
                return Unauthorized();
            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new DailyForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
        }

        [HttpGet]
        [Route("hourlyforecast")]

        public async Task<object> GetHourly()
        {
            var allowed = await _authorization.AuthorizeAsync(User, "HasProtectedAccess");
            if (!allowed.Succeeded)
                return Unauthorized();

            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new HourlyForecast
            {
                Time = DateTime.Now.AddHours(index),
                TemperatureC = rng.Next(-20, 55)
            })
            .ToArray();
        }
    }
}
