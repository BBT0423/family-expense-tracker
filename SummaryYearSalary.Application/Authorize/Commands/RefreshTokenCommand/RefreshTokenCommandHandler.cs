using SummaryYearSalary.Application.Common;
using SummaryYearSalary.Application.Interfaces.IServices;

using MediatR;

namespace SummaryYearSalary.Application.Authorize.Commands.RefreshTokenCommand;

public class RefreshTokenCommandHandler(IAuthorizeService authorizeService) : IRequestHandler<RefreshTokenCommand, Result<AuthorizeResponse>>
{
    private readonly IAuthorizeService _authorizeService = authorizeService;
    public async Task<Result<AuthorizeResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        return await _authorizeService.RefreshTokenAsync(request, cancellationToken);
    }
}