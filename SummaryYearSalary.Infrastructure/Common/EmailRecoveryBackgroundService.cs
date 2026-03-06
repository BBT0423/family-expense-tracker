using SummaryYearSalary.Application.Interfaces.IRepositories;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SummaryYearSalary.Infrastructure.Common;

public class EmailRecoveryBackgroundService(ILogger<EmailRecoveryBackgroundService> logger, IServiceProvider serviceProvider) : BackgroundService
{
    private readonly ILogger<EmailRecoveryBackgroundService> _logger = logger;
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Email Recovery Background Service is starting."); 
        while (!stoppingToken.IsCancellationRequested)
        {
            //// Create a new scope
            using var scope = _serviceProvider.CreateScope();
            //Your service
            var scopedProcessingService = scope.ServiceProvider.GetRequiredService<EmailRetryHandler>();
            var retryItem = await scopedProcessingService.DequeueAsync(stoppingToken);
            if (retryItem != null)
            {
                // Dispose recoveryEmails if it implements IDisposable
                using (retryItem.Mail as IDisposable)
                {
                    // Delay 3 seconds before retrying
                    await Task.Delay(3000, stoppingToken);

                    var scopedProcessingEmailService = scope.ServiceProvider.GetRequiredService<IEmailRepository>();
                    await scopedProcessingEmailService.SendEmailAsync(retryItem.Mail, retryItem.RetryCount + 1, stoppingToken);
                }
            }
        }

        _logger.LogInformation("Email Recovery Background Service is stopping.");
    }
}