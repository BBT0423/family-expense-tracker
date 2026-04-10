using SummaryYearSalary.Application.Common;
using SummaryYearSalary.Application.Interfaces;
using SummaryYearSalary.Application.Interfaces.IRepositories;

using MediatR;

namespace SummaryYearSalary.Application.Reports.Queries.ExportMonthlyReportQuery;

public class ExportMonthlyReportQueryHandler(
    ITransactionRepository transactionRepository,
    IExcelExportService excelExportService
) : IRequestHandler<ExportMonthlyReportQuery, Result<byte[]>>
{
    private readonly ITransactionRepository _transactionRepository = transactionRepository;
    private readonly IExcelExportService _excelExportService = excelExportService;

    public async Task<Result<byte[]>> Handle(ExportMonthlyReportQuery request, CancellationToken cancellationToken)
    {
        var transactions = await _transactionRepository.GetByUserMonthYearAsync(request.UserId, request.Month, request.Year);
        var bytes = _excelExportService.GenerateMonthlyReport(transactions, request.Month, request.Year);
        return Result.Success(bytes);
    }
}
