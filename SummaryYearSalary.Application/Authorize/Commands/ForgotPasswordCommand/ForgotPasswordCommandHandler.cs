using SummaryYearSalary.Application.Common;
using SummaryYearSalary.Application.Interfaces.IServices;

using MediatR;

namespace SummaryYearSalary.Application.Authorize.Commands.ForgotPasswordCommand;

public class ForgotPasswordCommandHandler(IAuthorizeService authorizeService) : IRequestHandler<ForgotPasswordCommand, Result<bool>>
{
    private readonly IAuthorizeService _authorizeService = authorizeService;
    public async Task<Result<bool>> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return await _authorizeService.ForgotPasswordAsync(request, cancellationToken);
    }
}