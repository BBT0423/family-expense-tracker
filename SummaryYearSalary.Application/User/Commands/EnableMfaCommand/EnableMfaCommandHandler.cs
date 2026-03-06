using SummaryYearSalary.Application.Common;
using SummaryYearSalary.Application.Interfaces.IServices;

using MediatR;

namespace SummaryYearSalary.Application.User.Commands.EnableMfaCommand;

public class EnableMfaCommandHandler(IAuthorizeService authorizeService) : IRequestHandler<EnableMfaCommand, Result<EnableMfaResponse>>
{
    private readonly IAuthorizeService _authorizeService = authorizeService;
    public async Task<Result<EnableMfaResponse>> Handle(EnableMfaCommand request, CancellationToken cancellationToken)
    {
        return await _authorizeService.EnableMfaAsync(request.UserId, cancellationToken);
    }
}