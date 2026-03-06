using SummaryYearSalary.Domain.Common;

namespace SummaryYearSalary.Domain.Entities;

/// <summary>
/// Audit log for tracking API usage and security events.
/// Helps with OWASP #9 - Improper Inventory Management.
/// </summary>
public class AuditLog
{
    public required string Id { get; set; }
    public string? UserId { get; set; }
    public string? UserName { get; set; }
    public string Action { get; set; } = string.Empty;
    public string Resource { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTimeProvider.GetBangkokTimeZone();
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}
