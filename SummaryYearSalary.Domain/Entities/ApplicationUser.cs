using SummaryYearSalary.Domain.Common;
using Microsoft.AspNetCore.Identity;

namespace SummaryYearSalary.Domain.Entities;

public class ApplicationUser : IdentityUser<string>
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateTime CreatedAt { get; set; } = DateTimeProvider.GetBangkokTimeZone();
    public DateTime? LastLoginAt { get; set; }
    
    // MFA Support (OWASP #2 - Broken Authentication)
    public bool IsMfaEnabled { get; set; }
    public string? MfaSecret { get; set; }
    
    // Account Security
    public int FailedLoginAttempts { get; set; }
    public DateTime? LockoutEndDate { get; set; }

    // Phone OTP Login
    public string? PhoneOtpCode { get; set; }
    public DateTime? PhoneOtpExpiry { get; set; }
    public int PhoneOtpRequestCount { get; set; }
    public DateTime? PhoneOtpWindowStart { get; set; }

    // Refresh Token Support
    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = [];
}