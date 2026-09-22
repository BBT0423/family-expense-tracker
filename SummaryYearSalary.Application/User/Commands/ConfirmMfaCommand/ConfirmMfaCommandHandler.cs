using SummaryYearSalary.Application.Common;
using SummaryYearSalary.Application.Interfaces.IServices;

using MediatR;

namespace SummaryYearSalary.Application.User.Commands.ConfirmMfaCommand;

public class ConfirmMfaCommandHandler(IAuthorizeService authorizeService) : IRequestHandler<ConfirmMfaCommand, Result<bool>>
{
    private readonly IAuthorizeService _authorizeService = authorizeService;

    public async Task<Result<bool>> Handle(ConfirmMfaCommand request, CancellationToken cancellationToken)
    {
        return await _authorizeService.ConfirmMfaAsync(request.UserId, request.Code, cancellationToken);
    }
}
