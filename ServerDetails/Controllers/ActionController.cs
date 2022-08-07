using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using System.Diagnostics;

namespace ServerDetails.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ActionController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;
        private readonly IHostApplicationLifetime _applicationLifetime;

        public ActionController(
            ILogger<ActionController> logger, 
            IConfiguration configuration,
            IHostApplicationLifetime applicationLifetime)
        {
            _logger = logger;
            _configuration = configuration;
            _applicationLifetime = applicationLifetime;
        }

        [HttpGet("grace-term")]
        public ActionResult<string> GraceTerm()
        {
            _logger.LogInformation("Executing /action/grace-term endpoint");

            _applicationLifetime.StopApplication();
            return "Response from /action/grace-term endpoint";
        }

        [HttpGet("non-grace-term")]
        public ActionResult<string> NonGraceTerm()
        {
            _logger.LogInformation("Executing /action/non-grace-term endpoint");

            Environment.Exit(-1);
            return "Response from /action/non-grace-term endpoint";
        }

        [HttpGet("app-execption")]
        public ActionResult AppException()
        {
            _logger.LogInformation("Executing /action/app-execption endpoint");

            throw new Exception("App Exception");

        }


        [HttpGet("update-ready-state")]
        public ActionResult SetReadyState(string isReady)
        {
            _logger.LogInformation("Executing /action/app-execption endpoint");

            System.IO.File.WriteAllText("ready-state.txt", isReady);

            return new EmptyResult();
        }

        [HttpGet("is-ready")]
        public ActionResult<string> IsReady()
        {
            _logger.LogInformation($"Executing /action/is-ready endpoint @ {DateTime.Now:HH:mm:ss:ffff}");

            var isReady = "true";

            if (System.IO.File.Exists("ready-state.txt"))
            {
                isReady = System.IO.File.ReadAllText("ready-state.txt");
            }

            if(isReady == "false")
            {
                throw new Exception("System is not ready");
            }


            return "Response from /action/is-ready endpoint";

        }


        [HttpGet("consume-resources")]
        public async Task<ActionResult<object>> GetAsync(
            [FromQuery] int duration = 1,
            [FromQuery] int core = 1,
            [FromQuery] int ram = 10)
        {
            try
            {
                core = Math.Max(core, 1);
                core = Math.Min(core, 256);
                ram = Math.Max(1, ram);
                ram = Math.Min(10000, ram);
                duration = Math.Max(duration, 1);
                duration = Math.Min(duration, 300);

                var source = new CancellationTokenSource(TimeSpan.FromSeconds(duration));
                var cancellationToken = source.Token;
                var stopWatch = Stopwatch.StartNew();
                var tasks = (from i in Enumerable.Range(0, core)
                             let task = Task.Run(() => BusyWork(ram, cancellationToken), cancellationToken)
                             select task).ToArray();

                await Task.WhenAll(tasks);

                return new
                {
                    duration = duration,
                    numberOfCore = core,
                    ramInMB = ram,
                    realDuration = stopWatch.Elapsed
                };
            }
            catch (AggregateException ex)
            {
                var messages = from e in ex.InnerExceptions
                               select e.Message;

                return new
                {
                    exceptions = messages.ToArray()
                };
            }
            catch (Exception ex)
            {
                return new
                {
                    exception = ex.Message
                };
            }
        }


        private void BusyWork(int ram, CancellationToken cancellationToken)
        {
            var ramChunk = new byte[ram * 1024 * 1024];
            var random = new Random();
            int i = 0;

            while (!cancellationToken.IsCancellationRequested)
            {
                ramChunk[i] = (byte)random.Next(0, 255);
                ++i;
                if (i >= ramChunk.Length)
                {
                    i = 0;
                }
            }
        }
    }
}
