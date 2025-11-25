using System.Net;
using SimpleLauncher.AdminAPI.Services;

namespace SimpleLauncher.AdminAPI.Middleware;

public class GlobalExceptionHandler
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(RequestDelegate next, ILogger<GlobalExceptionHandler> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IBugReportService bugReportService)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            // Original: _logger.LogError(ex, "An unhandled exception has occurred.");
            Log.UnhandledException(_logger, ex);

            // Send the bug report in the background without waiting
            _ = Task.Run(() => bugReportService.SendBugReportAsync(ex));

            // Return a generic error response to the client
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var response = new
            {
                error = "An unexpected internal server error has occurred.",
                traceId = context.TraceIdentifier
            };

            await context.Response.WriteAsJsonAsync(response);
        }
    }
}
