using SummaryYearSalary.Application.Common;
using SummaryYearSalary.Application.Notifications.Responses;

using MediatR;

namespace SummaryYearSalary.Application.Notifications.Queries.GetNotificationsQuery;

public record GetNotificationsQuery(string UserId, bool UnreadOnly = false) : IRequest<Result<IEnumerable<NotificationResponse>>>;
