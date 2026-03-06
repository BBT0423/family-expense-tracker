using System.Collections.Concurrent;

using SummaryYearSalary.Api.Helpers;
using SummaryYearSalary.Domain.Common;

namespace SummaryYearSalary.Api.Middlewares;

/// <summary>
/// Request validation middleware to prevent common attacks.
/// Addresses multiple OWASP vulnerabilities including injection attacks.
/// </summary>
public class RequestValidationMiddleware(RequestDelegate next, ILogger<RequestValidationMiddleware> logger)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<RequestValidationMiddleware> _logger = logger;
    
    // Track suspicious activity
    private static readonly ConcurrentDictionary<string, SuspiciousActivityTracker> _suspiciousIps = new();

    public async Task InvokeAsync(HttpContext context)
    {
        var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? ApiConstants.Unknown;

        // Check for suspicious patterns in query strings and headers
        if (ContainsSuspiciousPatterns(context))
        {
            _logger.LogWarning("Suspicious request detected from {IpAddress}: {Path}", ipAddress, context.Request.Path);

            TrackSuspiciousActivity(ipAddress);
            
            context.Response.StatusCode = 400;
            await context.Response.WriteAsJsonAsync(new { error = "Invalid request" });
            return;
        }

        // Check request size to prevent resource exhaustion (OWASP #4)
        if (context.Request.ContentLength > 10 * 1024 * 1024) // 10MB limit
        {
            _logger.LogWarning("Request too large from {IpAddress}: {Size} bytes", ipAddress, context.Request.ContentLength);
            context.Response.StatusCode = 413;
            await context.Response.WriteAsJsonAsync(new { error = "Request too large" });
            return;
        }

        // Check if IP is blocked due to suspicious activity
        if (IsBlocked(ipAddress))
        {
            _logger.LogWarning("Blocked request from {IpAddress}", ipAddress);
            context.Response.StatusCode = 403;
            await context.Response.WriteAsJsonAsync(new { error = "Access denied" });
            return;
        }

        await _next(context);
    }

    private static bool ContainsSuspiciousPatterns(HttpContext context)
    {
        var suspiciousPatterns = new[]
        {
            "../", "..\\",           // Path traversal
            "<script", "</script>",  // XSS
            "javascript:",           // XSS
            "onerror=", "onload=",   // XSS
            "exec(", "eval(",        // Code injection
            "union select",          // SQL injection
            "' or '1'='1",          // SQL injection
            "DROP TABLE",            // SQL injection
            "${", "#{",              // Expression language injection
        };

        var queryString = context.Request.QueryString.ToString().ToLowerInvariant();
        var path = context.Request.Path.ToString().ToLowerInvariant();
        
        if (suspiciousPatterns.Any(pattern => 
            queryString.Contains(pattern, StringComparison.InvariantCultureIgnoreCase) || 
            path.Contains(pattern, StringComparison.InvariantCultureIgnoreCase)))
        {
            return true;
        }

        // Check headers for suspicious content
        foreach (var header in context.Request.Headers)
        {
            var headerValue = header.Value.ToString().ToLowerInvariant();
            if (suspiciousPatterns.Any(p => headerValue.Contains(p, StringComparison.InvariantCultureIgnoreCase)))
            {
                return true;
            }
        }

        return false;
    }

    private static void TrackSuspiciousActivity(string ipAddress)
    {
        var tracker = _suspiciousIps.GetOrAdd(ipAddress, _ => new SuspiciousActivityTracker());
        tracker.RecordAttempt();
    }

    private static bool IsBlocked(string ipAddress)
    {
        return _suspiciousIps.TryGetValue(ipAddress, out var tracker) && tracker.IsCurrentlyBlocked();
    }

    private sealed class SuspiciousActivityTracker
    {
        private readonly List<DateTime> _attempts = [];
        private DateTime? _blockedUntil;
        private const int MaxAttemptsPerMinute = 10;
        private const int BlockDurationMinutes = 15;

        public void RecordAttempt()
        {
            lock (_attempts)
            {
                _attempts.Add(DateTimeProvider.GetBangkokTimeZone());
                
                // Clean old attempts
                _attempts.RemoveAll(a => a < DateTimeProvider.GetBangkokTimeZone().AddMinutes(-1));

                // Block if too many attempts
                if (_attempts.Count > MaxAttemptsPerMinute)
                {
                    _blockedUntil = DateTimeProvider.GetBangkokTimeZone().AddMinutes(BlockDurationMinutes);
                }
            }
        }

        public bool IsCurrentlyBlocked()
        {
            if (_blockedUntil.HasValue && _blockedUntil > DateTimeProvider.GetBangkokTimeZone())
            {
                return true;
            }
            _blockedUntil = null;
            return false;
        }
    }
}

public static class RequestValidationMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestValidation(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RequestValidationMiddleware>();
    }
}
