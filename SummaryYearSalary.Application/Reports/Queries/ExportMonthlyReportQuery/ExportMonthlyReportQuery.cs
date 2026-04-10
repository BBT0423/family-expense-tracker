using SummaryYearSalary.Application.Common;

using MediatR;

namespace SummaryYearSalary.Application.Reports.Queries.ExportMonthlyReportQuery;

public record ExportMonthlyReportQuery(string UserId, int Month, int Year) : IRequest<Result<byte[]>>;
