using System.Net;
using System.Text.Json;

namespace LibraryApi.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        private readonly IHostEnvironment _environment;
        private static readonly JsonSerializerOptions CachedJsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment environment)
        {
            _next = next;
            _logger = logger;
            _environment = environment;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                // Log detailed exception info to console
                LogExceptionDetails(httpContext, ex);

                // Handle the exception for the HTTP response
                await HandleExceptionAsync(httpContext, ex);
            }
        }

        private void LogExceptionDetails(HttpContext context, Exception exception)
        {
            // Include request details in the log
            _logger.LogError(
                "EXCEPTION: {ExceptionType} occurred processing {Path}\n" +
                "Message: {Message}\n" +
                "Request Method: {Method}\n" +
                "Request Query: {Query}\n" +
                "Stack Trace: {StackTrace}",
                exception.GetType().Name,
                context.Request.Path,
                exception.Message,
                context.Request.Method,
                context.Request.QueryString,
                exception.StackTrace);

            // Log inner exception details if present
            if (exception.InnerException != null)
            {
                _logger.LogError(
                    "INNER EXCEPTION: {ExceptionType}\n" +
                    "Message: {Message}\n" +
                    "Stack Trace: {StackTrace}",
                    exception.InnerException.GetType().Name,
                    exception.InnerException.Message,
                    exception.InnerException.StackTrace);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            context.Response.StatusCode = exception switch
            {
                KeyNotFoundException => (int)HttpStatusCode.NotFound,
                UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
                ArgumentException => (int)HttpStatusCode.BadRequest,
                InvalidOperationException => (int)HttpStatusCode.BadRequest,
                _ => (int)HttpStatusCode.InternalServerError
            };

            var response = _environment.IsDevelopment()
                ? new ApiException(context.Response.StatusCode, exception.Message, exception.StackTrace?.ToString())
                : new ApiException(context.Response.StatusCode, "Internal Server Error");

            var json = JsonSerializer.Serialize(response, CachedJsonSerializerOptions);

            await context.Response.WriteAsync(json);
        }
    }

    public class ApiException
    {
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public string Details { get; set; }

        public ApiException(int statusCode, string message, string details = null)
        {
            StatusCode = statusCode;
            Message = message;
            Details = details;
        }
    }
}