using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http.Controllers;

namespace MockCbsService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LoanManagerController : ControllerBase
    {
        private static readonly string[] Names = new[]
        {
            "Johnny Cecotto", "Brad Bracing", "Mary Chilly", "Emilio Cool", "Anthony Mild", "Gregory Warm", "Helmuth Balmy", "John Hot", "Adela Sweltering", "Trudy Scorching"
        };

        private readonly ILogger<LoanManagerController> _logger;

        public LoanManagerController(ILogger<LoanManagerController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        [MiddlewareFilter(typeof(CustomAuthenticationMiddlewarePipeline))]
        public IEnumerable<LoanAccount> Get()
        {
            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new LoanAccount
            {
                Date = DateTime.Now.AddDays(index),
                LoanBalance = rng.Next(5000, 23000),
                LoanMonths = rng.Next(15, 37),
                Name = Names[rng.Next(10)]
            })
            .ToArray();
        }

    }
}
