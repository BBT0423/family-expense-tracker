using System.Net.Mail;

namespace SummaryYearSalary.Application.Interfaces.IRepositories;

public interface IEmailRepository
{
    Task SendEmailAsync(MailMessage mail, CancellationToken cancellationToken = default);
    Task SendEmailAsync(MailMessage mail, int retryCount, CancellationToken cancellationToken = default);
    void SendEmail(MailMessage mail);
}