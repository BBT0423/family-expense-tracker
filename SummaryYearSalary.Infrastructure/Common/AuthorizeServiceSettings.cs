namespace SummaryYearSalary.Infrastructure.Common;

/// <summary>
/// Consolidated settings for AuthorizeService to reduce constructor parameter count.
/// </summary>
public class AuthorizeServiceSettings
{
    public SecuritySettings Security { get; set; } = null!;
    public JwtSettings Jwt { get; set; } = null!;
    public EmailSettings Email { get; set; } = null!;
}
