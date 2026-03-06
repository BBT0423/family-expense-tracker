using SummaryYearSalary.Application.Common;

using MediatR;

namespace SummaryYearSalary.Application.Authorize.Commands.ResetPasswordCommand;

public record ResetPasswordCommand(string Email, string Token, string NewPassword, string IpAddress, string Template) : IRequest<Result<bool>>;