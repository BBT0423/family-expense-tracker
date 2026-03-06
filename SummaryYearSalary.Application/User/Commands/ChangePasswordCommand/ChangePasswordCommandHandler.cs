using SummaryYearSalary.Application.Common;
using SummaryYearSalary.Application.Interfaces.IServices;

using MediatR;

namespace SummaryYearSalary.Application.User.Commands.ChangePasswordCommand;

public class ChangePasswordCommandHandler(IAuthorizeService authorizeService) : IRequestHandler<ChangePasswordCommand, Result<bool>>
{
    private readonly IAuthorizeService _authorizeService = authorizeService;
    public async Task<Result<bool>> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
       return await _authorizeService.ChangePasswordAsync(request, cancellationToken);
    }
}