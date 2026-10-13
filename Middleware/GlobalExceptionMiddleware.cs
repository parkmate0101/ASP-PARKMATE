using System.Text;
using System.Text.Json;

namespace ParkMate.Middleware
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;
        private readonly IWebHostEnvironment _environment;

        public GlobalExceptionMiddleware(
            RequestDelegate next,
            ILogger<GlobalExceptionMiddleware> logger,
            IWebHostEnvironment environment)
        {
            _next = next;
            _logger = logger;
            _environment = environment;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unhandled exception while processing {Path}",
                    context.Request.Path);

                if (context.Response.HasStarted)
                {
                    throw;
                }

                if (context.Request.Path.StartsWithSegments("/api"))
                {
                    await WriteApiErrorAsync(context);
                    return;
                }

                if (_environment.IsDevelopment())
                {
                    context.Response.Clear();
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    context.Response.ContentType = "text/plain; charset=utf-8";

                    var details = new StringBuilder();
                    details.AppendLine("ParkMate Development Exception");
                    details.AppendLine(new string('=', 40));
                    details.AppendLine();
                    details.AppendLine($"Path: {context.Request.Path}");
                    details.AppendLine($"Exception: {ex.GetType().FullName}");
                    details.AppendLine($"Message: {ex.Message}");
                    details.AppendLine();
                    details.AppendLine("Stack Trace:");
                    details.AppendLine(ex.ToString());

                    if (ex.InnerException != null)
                    {
                        details.AppendLine();
                        details.AppendLine("Inner Exception:");
                        details.AppendLine(ex.InnerException.ToString());
                    }

                    await context.Response.WriteAsync(details.ToString());
                    return;
                }

                context.Response.Redirect("/Home/Error");
            }
        }

        private static async Task WriteApiErrorAsync(HttpContext context)
        {
            context.Response.Clear();
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json; charset=utf-8";

            var payload = new
            {
                success = false,
                message = "An unexpected server error occurred."
            };

            await context.Response.WriteAsync(
                JsonSerializer.Serialize(payload));
        }
    }
}
