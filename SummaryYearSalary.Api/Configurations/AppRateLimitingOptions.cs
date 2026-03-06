namespace SummaryYearSalary.Api.Configurations;

public class AppRateLimitingOptions
{
    public const string SectionName = "RateLimiting";
    public int PermitLimit { get; set; }
    public int WindowSeconds { get; set; }
    public int QueueLimit { get; set; }
    public bool AutoReplenishment { get; set; }
}