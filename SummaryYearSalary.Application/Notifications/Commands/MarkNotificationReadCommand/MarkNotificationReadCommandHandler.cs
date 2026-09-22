using SummaryYearSalary.Application.Common;
using SummaryYearSalary.Application.Interfaces;
using SummaryYearSalary.Application.Interfaces.IRepositories;

using MediatR;

namespace SummaryYearSalary.Application.Notifications.Commands.MarkNotificationReadCommand;

public class MarkNotificationReadCommandHandler(
    INotificationRepository notificationRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<MarkNotificationReadCommand, Result<bool>>
{
    private readonly INotificationRepository _notificationRepository = notificationRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<Result<bool>> Handle(MarkNotificationReadCommand request, CancellationToken cancellationToken)
    {
        var notification = await _notificationRepository.GetByIdAsync(request.Id);
        if (notification is null || notification.UserId != request.UserId)
            return Result.Failure<bool>("ไม่พบการแจ้งเตือนนี้", 404);

        notification.IsRead = true;
        await _notificationRepository.UpdateAsync(notification);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(true);
    }
}
