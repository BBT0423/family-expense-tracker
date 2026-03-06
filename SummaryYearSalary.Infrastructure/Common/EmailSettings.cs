namespace SummaryYearSalary.Infrastructure.Common;

public class EmailSettings
{
    public const string SectionName = "EmailSettings";
    public string? Web { get; set; }
    public string? From { get; set; }
    public string? SmtpServer { get; set; }
    public int? Port { get; set; }
    public string? UserName { get; set; }
    public string? Password { get; set; }
    public bool EnableEmailNotifications { get; set; } = true;
}