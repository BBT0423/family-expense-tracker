using SummaryYearSalary.Application.Common;
using SummaryYearSalary.Application.Interfaces.IRepositories;
using SummaryYearSalary.Application.Reports.Responses;
using SummaryYearSalary.Domain.Enums;

using MediatR;

namespace SummaryYearSalary.Application.Reports.Queries.GetMonthlySummaryQuery;

public class GetMonthlySummaryQueryHandler(
    ITransactionRepository transactionRepository
) : IRequestHandler<GetMonthlySummaryQuery, Result<MonthlySummaryResponse>>
{
    private readonly ITransactionRepository _transactionRepository = transactionRepository;

    public async Task<Result<MonthlySummaryResponse>> Handle(GetMonthlySummaryQuery request, CancellationToken cancellationToken)
    {
        var transactions = await _transactionRepository.GetByUserMonthYearAsync(request.UserId, request.Month, request.Year);
        var list = transactions.ToList();

        var totalIncome = list.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
        var totalExpense = list.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);

        return Result.Success(new MonthlySummaryResponse(
            totalIncome, totalExpense, totalIncome - totalExpense, list.Count));
    }
}
