using SummaryYearSalary.Application.Common;
using SummaryYearSalary.Application.Interfaces.IServices;

using MediatR;

namespace SummaryYearSalary.Application.Authorize.Commands.RegisterCommand;

public class RegisterCommandHandler(
    IAuthorizeService authorizeService
) : IRequestHandler<RegisterCommand, Result<AuthorizeResponse>>
{
    private readonly IAuthorizeService _authorizeService = authorizeService;

    public async Task<Result<AuthorizeResponse>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        return await _authorizeService.RegisterAsync(request.Email, request.Password, request.FirstName, request.LastName, request.IpAddress, cancellationToken);
    }
}
