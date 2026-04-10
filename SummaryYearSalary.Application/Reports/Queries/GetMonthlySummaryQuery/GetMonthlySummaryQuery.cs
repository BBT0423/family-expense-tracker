using SummaryYearSalary.Application.Common;
using SummaryYearSalary.Application.Reports.Responses;

using MediatR;

namespace SummaryYearSalary.Application.Reports.Queries.GetMonthlySummaryQuery;

public record GetMonthlySummaryQuery(string UserId, int Month, int Year) : IRequest<Result<MonthlySummaryResponse>>;
