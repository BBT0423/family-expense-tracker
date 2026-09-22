using SummaryYearSalary.Application.Common;

using MediatR;

namespace SummaryYearSalary.Application.Notifications.Commands.MarkAllNotificationsReadCommand;

public record MarkAllNotificationsReadCommand(string UserId) : IRequest<Result<bool>>;
