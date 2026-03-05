using Ghasele.Domain.Entities;
using Ghasele.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Ghasele.API.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger, IServiceScopeFactory serviceScopeFactory)
        {
            _next = next;
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
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

        private async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred.");

            // Get full nested exception message
            string fullMessage = ex.Message;
            var inner = ex.InnerException;
            while (inner != null)
            {
                fullMessage += " --> " + inner.Message;
                inner = inner.InnerException;
            }

            // Create scope to resolve DbContext
            try
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    var auditLog = new AuditLog
                    {
                        UserId = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                        RequestPath = context.Request.Path,
                        RequestMethod = context.Request.Method,
                        ExceptionMessage = fullMessage,
                        StackTrace = ex.StackTrace,
                        StatusCode = (int)HttpStatusCode.InternalServerError
                    };

                    dbContext.AuditLogs.Add(auditLog);
                    await dbContext.SaveChangesAsync();
                }
            }
            catch (Exception logEx)
            {
                _logger.LogError(logEx, "Failed to log exception to database.");
            }

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var errorDetails = new ErrorDetails()
            {
                StatusCode = context.Response.StatusCode,
                Message = fullMessage // Providing full message for debugging
            };

            await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(errorDetails, new System.Text.Json.JsonSerializerOptions 
            { 
                PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase 
            }));
        }
    }

    public class ErrorDetails
    {
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
