using SummaryYearSalary.Application.Common;

using MediatR;

namespace SummaryYearSalary.Application.Authorize.Commands.RefreshTokenCommand;

public record RefreshTokenCommand(
    string RefreshToken, 
    string IpAddress
) : IRequest<Result<AuthorizeResponse>>;
