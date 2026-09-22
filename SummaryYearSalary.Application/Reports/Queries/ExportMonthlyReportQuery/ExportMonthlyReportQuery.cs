using SummaryYearSalary.Application.Common;

using MediatR;

namespace SummaryYearSalary.Application.Reports.Queries.ExportMonthlyReportQuery;

public record ExportMonthlyReportQuery(string UserId, int Month, int Year, string? ScopeUserId = null) : IRequest<Result<byte[]>>;
