using SummaryYearSalary.Application.Common;

using MediatR;

namespace SummaryYearSalary.Application.User.Commands.ChangePasswordCommand;

[SensitiveRequest]
public record ChangePasswordCommand(
    string UserId,
    string CurrentPassword,
    string NewPassword,
    string IpAddress,
    string Template
) : IRequest<Result<bool>>;