using System.Net;
using System.Text.Json;
using ClonEbay_CoreAPI.Common.Models;
using ClonEbay_CoreAPI.Exceptions;

namespace ClonEbay_CoreAPI.Middlewares
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
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
            int statusCode;
            string message;
            object? errors = null;
            var traceId = context.TraceIdentifier;

            if (exception is AppException appException)
            {
                // 4xx Client Error - Handled domain/business logic error
                statusCode = appException.StatusCode;
                message = appException.Message;
                errors = appException.Errors;

                _logger.LogWarning(
                    "Client Exception [{StatusCode}] on {Method} {Path} | TraceId: {TraceId} | Message: {Message}",
                    statusCode, context.Request.Method, context.Request.Path, traceId, message);
            }
            else
            {
                // 5xx Server Error - Unhandled unexpected system error
                statusCode = (int)HttpStatusCode.InternalServerError;
                message = "Đã xảy ra lỗi máy chủ nội bộ. Vui lòng thử lại sau hoặc liên hệ quản trị viên.";

                _logger.LogError(exception,
                    "Unhandled System Exception [500] on {Method} {Path} | TraceId: {TraceId} | Exception: {ExceptionType} - {Message}",
                    context.Request.Method, context.Request.Path, traceId, exception.GetType().Name, exception.Message);
            }

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            var errorResponse = new ApiErrorResponse
            {
                Success = false,
                StatusCode = statusCode,
                Message = message,
                Errors = errors,
                Timestamp = DateTime.UtcNow,
                TraceId = traceId
            };

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };

            var jsonResponse = JsonSerializer.Serialize(errorResponse, jsonOptions);
            await context.Response.WriteAsync(jsonResponse);
        }
    }

    /// <summary>
    /// Extension method để đăng ký middleware dễ dàng trong Program.cs
    /// </summary>
    public static class GlobalExceptionMiddlewareExtensions
    {
        public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<GlobalExceptionMiddleware>();
        }
    }
}
