using SummaryYearSalary.Application.Common;
using SummaryYearSalary.Application.Interfaces.IServices;

using MediatR;

namespace SummaryYearSalary.Application.Authorize.Commands.LoginCommand;

public class LoginCommandHandler(
    IAuthorizeService authorizeService
) : IRequestHandler<LoginCommand, Result<AuthorizeResponse>>
{
    private readonly IAuthorizeService _authorizeService = authorizeService;
    public async Task<Result<AuthorizeResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var result = await _authorizeService.LoginAsync(request, cancellationToken);
        return result;
    }
}