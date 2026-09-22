using SummaryYearSalary.Application.Common;

using MediatR;

namespace SummaryYearSalary.Application.Notifications.Commands.MarkNotificationReadCommand;

public record MarkNotificationReadCommand(int Id, string UserId) : IRequest<Result<bool>>;
