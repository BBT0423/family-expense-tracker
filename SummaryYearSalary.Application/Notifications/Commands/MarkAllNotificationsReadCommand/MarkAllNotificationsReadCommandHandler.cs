using SummaryYearSalary.Application.Common;
using SummaryYearSalary.Application.Interfaces.IRepositories;

using MediatR;

namespace SummaryYearSalary.Application.Notifications.Commands.MarkAllNotificationsReadCommand;

public class MarkAllNotificationsReadCommandHandler(
    INotificationRepository notificationRepository
) : IRequestHandler<MarkAllNotificationsReadCommand, Result<bool>>
{
    private readonly INotificationRepository _notificationRepository = notificationRepository;

    public async Task<Result<bool>> Handle(MarkAllNotificationsReadCommand request, CancellationToken cancellationToken)
    {
        await _notificationRepository.MarkAllAsReadAsync(request.UserId);
        return Result.Success(true);
    }
}
