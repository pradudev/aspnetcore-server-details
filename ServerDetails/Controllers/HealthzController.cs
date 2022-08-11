using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
namespace ServerDetails.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class HealthzController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;

        public HealthzController(ILogger<HealthzController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        [HttpGet]
        public ActionResult<string> Get()
        {
            _logger.LogInformation($"Executing /healthz endpoint @ {DateTime.Now:HH:mm:ss:ffff}");

            var responseSec = 1;

            if (System.IO.File.Exists("health-response-time-secs.txt"))
            {
                responseSec = Convert.ToInt32(System.IO.File.ReadAllText("health-response-time-secs.txt"));
            }

            Task.Delay(responseSec * 1000).Wait();

            return "Healthy";

        }
    }
}
