namespace SummaryYearSalary.Api.Configurations;

public class SecurityOptions
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