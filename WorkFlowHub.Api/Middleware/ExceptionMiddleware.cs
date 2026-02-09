using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace WorkFlowHub.Api.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        private readonly IWebHostEnvironment _env;

        public ExceptionMiddleware(
            RequestDelegate next, 
            ILogger<ExceptionMiddleware> logger,
            IWebHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            // Log the exception
            _logger.LogError(exception, "An error occurred: {Message}", exception.Message);

            // Set response type
            context.Response.ContentType = "application/json";

            // Determine status code and message based on exception type
            var (statusCode, message, shouldExposeDetails) = exception switch
            {
                // 400 Bad Request - Client errors
                InvalidOperationException => (400, exception.Message, true),
                ArgumentException => (400, exception.Message, true),

                // 401 Unauthorized
                UnauthorizedAccessException => (401, "You are not authorized to access this resource", true),

                // 404 Not Found
                KeyNotFoundException => (404, "The requested resource was not found", true),

                // 409 Conflict - Database constraints
                DbUpdateException dbEx when dbEx.InnerException?.Message.Contains("UNIQUE") == true 
                    => (409, "A record with this value already exists", false),
                
                DbUpdateException => (500, "A database error occurred", false),

                // 500 Internal Server Error - Unexpected errors
                _ => (500, "An unexpected error occurred", false)
            };

            context.Response.StatusCode = statusCode;

            // Create response object
            var response = new
            {
                statusCode,
                message,
                // Only include details in development environment
                details = _env.IsDevelopment() && shouldExposeDetails ? exception.StackTrace : null,
                timestamp = DateTime.UtcNow
            };

            var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(json);
        }
    }
}
