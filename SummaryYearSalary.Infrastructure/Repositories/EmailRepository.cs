using System.Net.Mail;

using FluentEmail.Core;

using SummaryYearSalary.Application.Interfaces;
using SummaryYearSalary.Infrastructure.Common;
using SummaryYearSalary.Application.Interfaces.IRepositories;

namespace SummaryYearSalary.Infrastructure.Repositories;

public class EmailRepository(IFluentEmail fluentEmail, IAppLogger logger, EmailRetryHandler emailRetryQueue) : IEmailRepository
{
    private readonly IFluentEmail _fluentEmail = fluentEmail;
    private readonly IAppLogger _logger = logger;
    private readonly EmailRetryHandler _emailRetryQueue = emailRetryQueue;

    public void SendEmail(MailMessage mail)
    {
        try
        {
            if (mail == null || mail.To.Count == 0)
            {
                return;
            }

            var to = string.Join(";", mail.To.Select(x => x.Address));
            _fluentEmail.To(to)
                              .Subject(mail.Subject)
                              .Body(mail.Body, isHtml: true)
                              .Send();
        }
        catch (SmtpException ex)
        {
            _logger.LogDebug(ex, $"Failed to send email synchronously. Error: {ex.Message}");
            _emailRetryQueue.Enqueue(mail);
        }
    }

    public async Task SendEmailAsync(MailMessage mail, CancellationToken cancellationToken = default)
    {
        var to = string.Empty;
        try
        {
            if (mail == null || mail.To.Count == 0)
            {
                await Task.CompletedTask;
                return;
            }
            to = string.Join(";", mail.To.Select(x => x.Address));
            var result = await _fluentEmail.To(to)
                              .Subject(mail.Subject)
                              .Body(mail.Body, isHtml: true)
                              .SendAsync(cancellationToken);

            if (!result.Successful)
            {
                _emailRetryQueue.Enqueue(mail);
            }

            _logger.LogDebug($"Email successfully sent to {to}.");
        }
        catch (SmtpException ex)
        {
            _emailRetryQueue.Enqueue(mail);
            // Log the email fail
            _logger.LogDebug(ex, $"Failed to send email to ({to}). Error: {ex.Message}");
        }

    }

    public async Task SendEmailAsync(MailMessage mail, int retryCount, CancellationToken cancellationToken = default)
    {
        var to = string.Empty;
        try
        {
            if (mail == null || mail.To.Count == 0)
            {
                await Task.CompletedTask;
                return;
            }
            to = string.Join(";", mail.To.Select(x => x.Address));
            var result = await _fluentEmail.To(to)
                              .Subject(mail.Subject)
                              .Body(mail.Body, isHtml: true)
                              .SendAsync(cancellationToken);

            if (!result.Successful)
            {
                _emailRetryQueue.Enqueue(mail, retryCount);
            }

            _logger.LogDebug($"Email successfully sent to {to}.");
        }
        catch (SmtpException ex)
        {
            _emailRetryQueue.Enqueue(mail, retryCount);
            // Log the email fail
            _logger.LogDebug(ex, $"Failed to send email to ({to}). Error: {ex.Message}");
        }
    }
}