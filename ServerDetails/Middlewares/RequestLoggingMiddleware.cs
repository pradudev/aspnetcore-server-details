using System.Diagnostics;

namespace ServerDetails.Middlewares
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var timer = new Stopwatch();
            timer.Start();

            _logger.LogInformation($"Request for {context.Request.Path} from IP: {context.Connection.RemoteIpAddress?.ToString()}");

            // Call the next delegate/middleware in the pipeline.
            await _next(context);

            timer.Stop();

            TimeSpan timeTaken = timer.Elapsed;

            _logger.LogInformation($"Response for {context.Request.Path} | Status: {context.Response.StatusCode} | ResponseTime: {timeTaken.TotalSeconds}s");
        }
    }

    public static class RequestLoggingMiddlewareExtensions
    {
        public static IApplicationBuilder UseRequestLogging(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RequestLoggingMiddleware>();
        }
    }
}
