using SummaryYearSalary.Application.Common;
using SummaryYearSalary.Application.Interfaces.IRepositories;
using SummaryYearSalary.Application.Reports.Responses;
using SummaryYearSalary.Domain.Enums;

using MediatR;

namespace SummaryYearSalary.Application.Reports.Queries.GetCategorySummaryQuery;

public class GetCategorySummaryQueryHandler(
    ITransactionRepository transactionRepository
) : IRequestHandler<GetCategorySummaryQuery, Result<CategorySummaryResponse>>
{
    private readonly ITransactionRepository _transactionRepository = transactionRepository;

    public async Task<Result<CategorySummaryResponse>> Handle(GetCategorySummaryQuery request, CancellationToken cancellationToken)
    {
        var transactions = await ReportScope.ResolveMonthYearAsync(_transactionRepository, request.UserId, request.ScopeUserId, request.Month, request.Year, request.MemberId);
        var expenses = transactions.Where(t => t.Type == TransactionType.Expense).ToList();
        var totalExpense = expenses.Sum(t => t.Amount);

        var categories = expenses
            .GroupBy(t => new { t.CategoryId, t.Category.Name, t.Category.Icon, t.Category.Color })
            .Select(g => new CategoryBreakdown(
                g.Key.CategoryId,
                g.Key.Name,
                g.Key.Icon,
                g.Key.Color,
                g.Sum(t => t.Amount),
                totalExpense > 0 ? Math.Round((double)(g.Sum(t => t.Amount) / totalExpense * 100), 1) : 0,
                g.Count()))
            .OrderByDescending(c => c.Total);

        return Result.Success(new CategorySummaryResponse(categories));
    }
}
