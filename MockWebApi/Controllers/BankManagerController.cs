using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MockWebApi.Middleware;
//using MockCbsService.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using MockWebApi.Policy;

namespace MockWebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize(Policy = "DefaultPolicy")]
    public class BankManagerController : ControllerBase
    {
        private readonly ILogger<BankManagerController> _logger;

        private static readonly string[] Names = new[]
        {
            "Johnny Cecotto", "Brad Bracing", "Mary Chilly", "Emilio Cool", "Anthony Mild", "Gregory Warm", "Helmuth Balmy", "John Hot", "Adela Sweltering", "Trudy Scorching"
        };

        public BankManagerController(ILogger<BankManagerController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        [Route("bankaccountslist")]
        [ContributorRoleAuthorize("Controller")]
        public IEnumerable<BankAccount> GetLoanList()
        {
            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new BankAccount
            {
                Date = DateTime.Now.AddDays(index),
                LoanBalance = rng.Next(5000, 23000),
                Account = rng.Next(15000, 15999),
                Name = Names[rng.Next(10)]
            })
            .ToArray();
        }

    }
}
