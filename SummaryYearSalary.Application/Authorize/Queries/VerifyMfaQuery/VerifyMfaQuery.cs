using SummaryYearSalary.Application.Common;

using MediatR;

namespace SummaryYearSalary.Application.Authorize.Queries.VerifyMfaQuery;

public record VerifyMfaQuery(string UserId, string MfaCode, string IpAddress) : IRequest<Result<AuthorizeResponse>>;