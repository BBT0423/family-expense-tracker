using SummaryYearSalary.Application.Common;
using SummaryYearSalary.Application.Interfaces.IRepositories;
using SummaryYearSalary.Application.Reports.Responses;
using SummaryYearSalary.Domain.Enums;

using MediatR;

namespace SummaryYearSalary.Application.Reports.Queries.GetMemberSummaryQuery;

public class GetMemberSummaryQueryHandler(
    ITransactionRepository transactionRepository
) : IRequestHandler<GetMemberSummaryQuery, Result<MemberSummaryResponse>>
{
    private readonly ITransactionRepository _transactionRepository = transactionRepository;

    public async Task<Result<MemberSummaryResponse>> Handle(GetMemberSummaryQuery request, CancellationToken cancellationToken)
    {
        var transactions = await _transactionRepository.GetByUserMonthYearAsync(request.UserId, request.Month, request.Year);
        var list = transactions.ToList();
        var totalExpense = list.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);

        var members = list
            .GroupBy(t => new { t.MemberId, t.Member.Name, t.Member.Color })
            .Select(g => new MemberBreakdown(
                g.Key.MemberId,
                g.Key.Name,
                g.Key.Color,
                g.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount),
                g.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount),
                totalExpense > 0
                    ? Math.Round((double)(g.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount) / totalExpense * 100), 1)
                    : 0))
            .OrderByDescending(m => m.TotalExpense);

        return Result.Success(new MemberSummaryResponse(members));
    }
}
