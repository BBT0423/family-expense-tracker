using SummaryYearSalary.Application.Common;
using SummaryYearSalary.Application.Interfaces.IServices;

using MediatR;

namespace SummaryYearSalary.Application.Authorize.Commands.SendPhoneOtpCommand;

public class SendPhoneOtpCommandHandler(
    IAuthorizeService authorizeService
) : IRequestHandler<SendPhoneOtpCommand, Result<bool>>
{
    private readonly IAuthorizeService _authorizeService = authorizeService;

    public async Task<Result<bool>> Handle(SendPhoneOtpCommand request, CancellationToken cancellationToken)
    {
        return await _authorizeService.SendPhoneOtpAsync(request, cancellationToken);
    }
}
