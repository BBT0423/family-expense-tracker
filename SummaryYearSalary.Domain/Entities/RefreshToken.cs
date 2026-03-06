using SummaryYearSalary.Domain.Common;

namespace SummaryYearSalary.Domain.Entities;

public class RefreshToken : BaseEntity
{
    public required string UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public string CreatedByIp { get; set; } = string.Empty;
    public DateTime? RevokedAt { get; set; }
    public string? RevokedByIp { get; set; }
    public string? ReplacedByToken { get; set; }
    public string? ReasonRevoked { get; set; }

    public bool IsExpired => DateTimeProvider.GetBangkokTimeZone() >= ExpiresAt;
    public bool IsRevoked => RevokedAt != null;
    public bool IsActive => !IsRevoked && !IsExpired;

    
    public virtual ApplicationUser User { get; set; } = null!;
}