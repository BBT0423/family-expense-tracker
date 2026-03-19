using System.Security.Claims;

using SummaryYearSalary.Api.Helpers;
using SummaryYearSalary.Application.Common;
using SummaryYearSalary.Application.Interfaces.IRepositories;
using SummaryYearSalary.Domain.Common;

namespace SummaryYearSalary.Api.Middlewares;

/// <summary>
/// Middleware for logging API requests and responses.
/// Supports OWASP #9 - Improper Inventory Management.
/// </summary>
public class AuditLoggingMiddleware(RequestDelegate next, ILogger<AuditLoggingMiddleware> logger)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<AuditLoggingMiddleware> _logger = logger;

    public async Task InvokeAsync(HttpContext context, IAuditLogRepository auditLogRepository)
    {
        var startTime = DateTimeProvider.GetBangkokTimeZone();

        try
        {
            await _next(context);

            // Log successful request
            await LogRequestAsync(context, auditLogRepository, true, null);
        }
        catch (Exception ex)
        {
            // Log failed request for audit trail; exception logging is handled by ExceptionHandlingMiddleware
            await LogRequestAsync(context, auditLogRepository, false, ex.Message);

            throw;
        }
        finally
        {
            var duration = DateTimeProvider.GetBangkokTimeZone() - startTime;
            _logger.LogInformation(
                "Request {Method} {Path} completed in {Duration}ms with status {StatusCode}",
                context.Request.Method,
                context.Request.Path,
                duration.TotalMilliseconds,
                context.Response.StatusCode);
        }
    }

    private static async Task LogRequestAsync(
        HttpContext context,
        IAuditLogRepository auditLogRepository,
        bool success,
        string? errorMessage)
    {
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userName = context.User.FindFirst(ClaimTypes.Name)?.Value ?? "Anonymous";
        var action = context.Request.Method;
        if (string.Equals(action, HttpMethods.Options, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }
        var resource = context.Request.Path.Value ?? "";
        var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? ApiConstants.Unknown;
        var userAgent = context.Request.Headers.UserAgent.ToString();

        await auditLogRepository.LogActionAsync(new AuditLogRequest
        {
            UserId = userId,
            UserName = userName,
            Action = action,
            Resource = resource,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            Success = success,
            ErrorMessage = errorMessage
        });
    }
}

public static class AuditLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseAuditLogging(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<AuditLoggingMiddleware>();
    }
}
