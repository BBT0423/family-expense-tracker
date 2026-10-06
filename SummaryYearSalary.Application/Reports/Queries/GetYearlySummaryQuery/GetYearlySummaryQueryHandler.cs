using SummaryYearSalary.Application.Common;
using SummaryYearSalary.Application.Interfaces.IRepositories;
using SummaryYearSalary.Application.Reports.Responses;
using SummaryYearSalary.Domain.Enums;

using MediatR;

namespace SummaryYearSalary.Application.Reports.Queries.GetYearlySummaryQuery;

public class GetYearlySummaryQueryHandler(
    ITransactionRepository transactionRepository
) : IRequestHandler<GetYearlySummaryQuery, Result<YearlySummaryResponse>>
{
    private readonly ITransactionRepository _transactionRepository = transactionRepository;

    public async Task<Result<YearlySummaryResponse>> Handle(GetYearlySummaryQuery request, CancellationToken cancellationToken)
    {
        var transactions = await ReportScope.ResolveYearAsync(_transactionRepository, request.UserId, request.ScopeUserId, request.Year, request.MemberId);
        var list = transactions.ToList();

        var months = Enumerable.Range(1, 12).Select(m => new MonthlyBreakdown(
            m,
            list.Where(t => t.TransactionDate.Month == m && t.Type == TransactionType.Income).Sum(t => t.Amount),
            list.Where(t => t.TransactionDate.Month == m && t.Type == TransactionType.Expense).Sum(t => t.Amount)
        ));

        return Result.Success(new YearlySummaryResponse(months));
    }
}
