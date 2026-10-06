namespace SummaryYearSalary.Api.Middlewares;

/// <summary>
/// Middleware to add security headers to all responses.
/// Addresses OWASP #8 - Security Misconfiguration.
/// </summary>
public class SecurityHeadersMiddleware(RequestDelegate next)
{
    private readonly RequestDelegate _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        // Remove server header to prevent information disclosure
        context.Response.Headers.Remove("Server");
        context.Response.Headers.Remove("X-Powered-By");

        // Strict-Transport-Security: Enforce HTTPS
        context.Response.Headers.Append("Strict-Transport-Security", "max-age=31536000; includeSubDomains");

        // X-Content-Type-Options: Prevent MIME-sniffing
        context.Response.Headers.Append("X-Content-Type-Options", "nosniff");

        // X-Frame-Options: Prevent clickjacking
        context.Response.Headers.Append("X-Frame-Options", "DENY");

        // X-XSS-Protection: Enable XSS filter
        context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");

        // Content-Security-Policy: Prevent XSS and data injection
        // style-src allows 'unsafe-inline' because Element Plus (the UI library)
        // applies inline styles at runtime for dynamic positioning/theming —
        // there's no practical way to nonce/hash those. font-src allows data:
        // for the icon font Element Plus embeds as a data: URI. script-src stays
        // 'self' only, so this doesn't open up script injection.
#pragma warning disable S7039 // unsafe-inline is required for style-src only, not script-src — see comment above
        context.Response.Headers.Append("Content-Security-Policy",
            "default-src 'self'; " +
            "script-src 'self'; " +
            "style-src 'self' 'unsafe-inline'; " +
            "img-src 'self' data:; " +
            "font-src 'self' data:; " +
            "connect-src 'self'; " +
            "frame-ancestors 'none'; " +
            "object-src 'none'; " +
            "base-uri 'self'");
#pragma warning restore S7039

        // Referrer-Policy: Control referrer information
        context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");

        // Permissions-Policy: Control browser features
        context.Response.Headers.Append("Permissions-Policy",
            "geolocation=(), microphone=(), camera=()");

        await _next(context);
    }
}

public static class SecurityHeadersMiddlewareExtensions
{
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<SecurityHeadersMiddleware>();
    }
}
