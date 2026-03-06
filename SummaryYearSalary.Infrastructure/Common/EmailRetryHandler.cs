using System.Collections.Concurrent;
using System.Net.Mail;

namespace SummaryYearSalary.Infrastructure.Common;

public class EmailRetryItem(MailMessage mail)
{
    public MailMessage Mail { get; set; } = mail;
    public int RetryCount { get; set; } = 0;
}

public class EmailRetryHandler : IDisposable
{
    private readonly ConcurrentQueue<EmailRetryItem> _failedEmails = new();
    private readonly SemaphoreSlim _signal = new(0);
    private const int MaxRetries = 3;

    public void Enqueue(MailMessage mail, int currentRetryCount = 0)
    {
        if (mail == null)
        {
            return;
        }

        if (currentRetryCount < MaxRetries)
        {
            var retryItem = new EmailRetryItem(mail) { RetryCount = currentRetryCount };
            _failedEmails.Enqueue(retryItem);
            _signal.Release();
        }
        else
        {
            // Max retries exceeded - discard the email
            mail.Dispose();
        }
    }

    public async Task<EmailRetryItem?> DequeueAsync(CancellationToken cancellationToken)
    {
        await _signal.WaitAsync(cancellationToken);

        return _failedEmails.TryDequeue(out var retryItem) ? retryItem : null;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _signal.Dispose();
            // Dispose all remaining emails
            while (_failedEmails.TryDequeue(out var item))
            {
                item.Mail?.Dispose();
            }
        }
    }
}
