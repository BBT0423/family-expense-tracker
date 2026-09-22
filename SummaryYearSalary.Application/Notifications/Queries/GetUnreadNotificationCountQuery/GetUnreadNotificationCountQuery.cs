using SummaryYearSalary.Application.Common;

using MediatR;

namespace SummaryYearSalary.Application.Notifications.Queries.GetUnreadNotificationCountQuery;

public record GetUnreadNotificationCountQuery(string UserId) : IRequest<Result<int>>;
