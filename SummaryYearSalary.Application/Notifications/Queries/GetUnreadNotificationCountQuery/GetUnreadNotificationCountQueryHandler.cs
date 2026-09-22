using SummaryYearSalary.Application.Common;
using SummaryYearSalary.Application.Interfaces.IRepositories;

using MediatR;

namespace SummaryYearSalary.Application.Notifications.Queries.GetUnreadNotificationCountQuery;

public class GetUnreadNotificationCountQueryHandler(
    INotificationRepository notificationRepository
) : IRequestHandler<GetUnreadNotificationCountQuery, Result<int>>
{
    private readonly INotificationRepository _notificationRepository = notificationRepository;

    public async Task<Result<int>> Handle(GetUnreadNotificationCountQuery request, CancellationToken cancellationToken)
    {
        var count = await _notificationRepository.GetUnreadCountAsync(request.UserId);
        return Result.Success(count);
    }
}
