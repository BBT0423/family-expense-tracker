using SummaryYearSalary.Application.Common;
using SummaryYearSalary.Application.Interfaces.IRepositories;
using SummaryYearSalary.Application.Notifications.Responses;

using MediatR;

namespace SummaryYearSalary.Application.Notifications.Queries.GetNotificationsQuery;

public class GetNotificationsQueryHandler(
    INotificationRepository notificationRepository
) : IRequestHandler<GetNotificationsQuery, Result<IEnumerable<NotificationResponse>>>
{
    private readonly INotificationRepository _notificationRepository = notificationRepository;

    public async Task<Result<IEnumerable<NotificationResponse>>> Handle(GetNotificationsQuery request, CancellationToken cancellationToken)
    {
        var notifications = await _notificationRepository.GetByUserIdAsync(request.UserId, request.UnreadOnly);
        var response = notifications.Select(n => new NotificationResponse(
            n.Id, n.Type.ToString(), n.Title, n.Message, n.RelatedEntityId, n.IsRead, n.CreatedAt));
        return Result.Success(response);
    }
}
