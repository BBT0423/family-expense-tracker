using SummaryYearSalary.Application.Common;
using SummaryYearSalary.Application.Interfaces.IServices;

using MediatR;

namespace SummaryYearSalary.Application.Authorize.Queries.VerifyMfaQuery;

public class VerifyMfaQueryHandler(IAuthorizeService authorizeService) : IRequestHandler<VerifyMfaQuery, Result<AuthorizeResponse>>
{
    private readonly IAuthorizeService _authorizeService = authorizeService;
    public async Task<Result<AuthorizeResponse>> Handle(VerifyMfaQuery request, CancellationToken cancellationToken)
    {
        return await _authorizeService.VerifyMfaAsync(request, cancellationToken);
    }
}