using Ghasele.API.Localization;
using Ghasele.Application.Exceptions;
using Ghasele.Application.Localization;
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

            // An AppException is a deliberate, user-facing error; anything else is a bug or an
            // infrastructure failure whose details must not reach the client.
            var appException = ex as AppException;
            var statusCode = appException?.StatusCode ?? (int)HttpStatusCode.InternalServerError;
            var errorCode = appException?.ErrorCode ?? ErrorCodes.InternalError;

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
                        // The audit trail keeps the untranslated detail for diagnosis.
                        ExceptionMessage = fullMessage,
                        StackTrace = ex.StackTrace,
                        StatusCode = statusCode
                    };

                    dbContext.AuditLogs.Add(auditLog);
                    await dbContext.SaveChangesAsync();
                }
            }
            catch (Exception logEx)
            {
                _logger.LogError(logEx, "Failed to log exception to database.");
            }

            if (context.Response.HasStarted)
            {
                // Headers are already on the wire; rewriting the response would corrupt it.
                _logger.LogWarning("Response already started; could not write error payload.");
                return;
            }

            var localizer = context.RequestServices.GetRequiredService<IRequestLocalizer>();
            var language = localizer.Language;

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;
            context.Response.Headers.ContentLanguage = language;

            var errorDetails = new ErrorDetails()
            {
                StatusCode = statusCode,
                ErrorCode = errorCode,
                Message = appException is not null
                    ? localizer.L(appException.ErrorCode, appException.Args)
                    : localizer.L(ErrorCodes.InternalError)
            };

            await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(errorDetails, new System.Text.Json.JsonSerializerOptions
            {
                PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
                // Arabic must survive serialization as readable UTF-8 rather than \uXXXX escapes.
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            }));
        }
    }

    public class ErrorDetails
    {
        public int StatusCode { get; set; }

        /// <summary>Stable machine-readable identifier, safe for clients to switch on.</summary>
        public string ErrorCode { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;
    }
}
