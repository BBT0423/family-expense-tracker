using SummaryYearSalary.Application.Common;

using MediatR;

namespace SummaryYearSalary.Application.Authorize.Commands.ForgotPasswordCommand;

public record ForgotPasswordCommand(string Email, string Template, string IpAddress) : IRequest<Result<bool>>;