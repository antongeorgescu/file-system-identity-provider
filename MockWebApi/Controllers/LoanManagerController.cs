using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
//using MockCbsService.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http.Controllers;

namespace MockCbsService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [MiddlewareFilter(typeof(CustomAuthorizationMiddlewarePipeline))]
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
        [Route("loanlist")]
        public IEnumerable<LoanAccount> GetLoanList()
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
