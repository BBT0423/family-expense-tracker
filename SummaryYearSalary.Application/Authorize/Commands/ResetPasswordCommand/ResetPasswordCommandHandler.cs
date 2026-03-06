using SummaryYearSalary.Application.Common;
using SummaryYearSalary.Application.Interfaces.IServices;

using MediatR;

namespace SummaryYearSalary.Application.Authorize.Commands.ResetPasswordCommand;

public class ResetPasswordCommandHandler(IAuthorizeService authorizeService) : IRequestHandler<ResetPasswordCommand, Result<bool>>
{
    private readonly IAuthorizeService _authorizeService = authorizeService;
    public async Task<Result<bool>> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        return await _authorizeService.ResetPasswordAsync(request, cancellationToken);
    }
}