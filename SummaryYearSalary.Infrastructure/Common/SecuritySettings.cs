namespace SummaryYearSalary.Infrastructure.Common;

/// <summary>
/// JWT configuration settings from appsettings.json.
/// </summary>
public class JwtSettings
{
    public const string SectionName = "JwtSettings";
    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int AccessTokenExpirationMinutes { get; set; } = 120;
    public int RefreshTokenExpirationDays { get; set; } = 7;
    public int ResetPasswordTokenExpiresMinutes { get; set; } = 30;

}

/// <summary>
/// Security configuration settings.
/// </summary>
public class SecuritySettings
{
    public const string SectionName = "SecuritySettings";
    public int MaxFailedLoginAttempts { get; set; } = 5;
    public int LockoutDurationMinutes { get; set; } = 15;
    public int MinPasswordLength { get; set; } = 8;
    public bool RequireDigit { get; set; } = true;
    public bool RequireUppercase { get; set; } = true;
    public bool RequireLowercase { get; set; } = true;
    public bool RequireNonAlphanumeric { get; set; } = true;
    public List<string> AllowedExternalDomains { get; set; } = [];
}

/// <summary>
/// Google OAuth configuration settings.
/// </summary>
public class GoogleAuthSettings
{
    public const string SectionName = "GoogleAuth";
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string CallbackUrl { get; set; } = string.Empty;
}

/// <summary>
/// Rate limiting configuration.
/// Addresses OWASP #4 - Unrestricted Resource Consumption.
/// </summary>
public class RateLimitSettings
{
    public const string SectionName = "RateLimitSettings";
    public int PermitLimit { get; set; } = 200;
    public int WindowSeconds { get; set; } = 60;
    public int QueueLimit { get; set; } = 10;
}