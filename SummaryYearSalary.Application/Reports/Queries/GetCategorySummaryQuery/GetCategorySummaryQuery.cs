using SummaryYearSalary.Application.Common;
using SummaryYearSalary.Application.Reports.Responses;

using MediatR;

namespace SummaryYearSalary.Application.Reports.Queries.GetCategorySummaryQuery;

public record GetCategorySummaryQuery(string UserId, int Month, int Year, string? ScopeUserId = null) : IRequest<Result<CategorySummaryResponse>>;
