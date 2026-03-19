using SummaryYearSalary.Application.Common;
using SummaryYearSalary.Application.Interfaces.IServices;

using MediatR;

namespace SummaryYearSalary.Application.Authorize.Commands.VerifyPhoneOtpCommand;

public class VerifyPhoneOtpCommandHandler(
    IAuthorizeService authorizeService
) : IRequestHandler<VerifyPhoneOtpCommand, Result<AuthorizeResponse>>
{
    private readonly IAuthorizeService _authorizeService = authorizeService;

    public async Task<Result<AuthorizeResponse>> Handle(VerifyPhoneOtpCommand request, CancellationToken cancellationToken)
    {
        return await _authorizeService.VerifyPhoneOtpAsync(request, cancellationToken);
    }
}
