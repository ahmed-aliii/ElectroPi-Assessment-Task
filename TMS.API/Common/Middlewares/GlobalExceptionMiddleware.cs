using System.Text.Json;
using TMS.Application;

namespace TMS.API
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;
        private readonly IWebHostEnvironment _env;

        public GlobalExceptionMiddleware(
            RequestDelegate next,
            ILogger<GlobalExceptionMiddleware> logger,
            IWebHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception caught by GlobalExceptionMiddleware");

                // In development let the DeveloperExceptionPage middleware handle it
                if (_env.IsDevelopment())
                {
                    throw;
                }

                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            ServiceResult<object> result;

            // Map common exceptions to meaningful ServiceResult responses
            if (exception is UnauthorizedAccessException)
            {
                result = ServiceResult<object>.Unauthorized(exception.Message);
            }
            else if (exception is ArgumentException || exception is ArgumentNullException || exception is FormatException)
            {
                result = ServiceResult<object>.BadRequest(exception.Message);
            }
            else if (exception is KeyNotFoundException)
            {
                result = ServiceResult<object>.NotFound(exception.Message);
            }
            else
            {
                // For unexpected exceptions, don't leak internal details in production
                var message = exception.Message;
                result = ServiceResult<object>.Failure(message);
            }

            context.Response.StatusCode = (int)result.StatusCode;

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var payload = new
            {
                success = result.Success,
                message = result.Messages,
                data = result.Data,
                statusCode = (int)result.StatusCode
            };

            return context.Response.WriteAsync(JsonSerializer.Serialize(payload, options));
        }
    }
}
