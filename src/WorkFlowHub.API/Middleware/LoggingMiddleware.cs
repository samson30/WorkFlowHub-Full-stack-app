using System.Diagnostics;

namespace WorkFlowHub.API.Middleware;

public class LoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<LoggingMiddleware> _logger;

    public LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var sw = Stopwatch.StartNew();

        _logger.LogInformation("Started {Method} {Path}",
            context.Request.Method, context.Request.Path);

        await _next(context);

        sw.Stop();

        _logger.LogInformation("Completed {Method} {Path} {StatusCode} in {ElapsedMs}ms",
            context.Request.Method, context.Request.Path,
            context.Response.StatusCode, sw.ElapsedMilliseconds);
    }
}
