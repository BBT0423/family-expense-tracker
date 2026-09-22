using SummaryYearSalary.Domain.Enums;

namespace SummaryYearSalary.Application.Interfaces;

public interface INotificationService
{
    Task NotifyAsync(
        string userId,
        NotificationType type,
        string title,
        string message,
        int? relatedEntityId = null,
        CancellationToken cancellationToken = default);
}
