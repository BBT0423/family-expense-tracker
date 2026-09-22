using SummaryYearSalary.Application.Common;
using SummaryYearSalary.Application.Reports.Responses;

using MediatR;

namespace SummaryYearSalary.Application.Reports.Queries.GetYearlySummaryQuery;

public record GetYearlySummaryQuery(string UserId, int Year, string? ScopeUserId = null) : IRequest<Result<YearlySummaryResponse>>;
