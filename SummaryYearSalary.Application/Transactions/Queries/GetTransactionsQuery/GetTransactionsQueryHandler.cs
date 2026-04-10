using SummaryYearSalary.Application.Common;
using SummaryYearSalary.Application.Interfaces.IRepositories;
using SummaryYearSalary.Application.Transactions.Responses;

using MediatR;

namespace SummaryYearSalary.Application.Transactions.Queries.GetTransactionsQuery;

public class GetTransactionsQueryHandler(
    ITransactionRepository transactionRepository
) : IRequestHandler<GetTransactionsQuery, Result<PaginatedList<TransactionResponse>>>
{
    private readonly ITransactionRepository _transactionRepository = transactionRepository;

    public async Task<Result<PaginatedList<TransactionResponse>>> Handle(GetTransactionsQuery request, CancellationToken cancellationToken)
    {
        var paged = await _transactionRepository.GetPagedByUserAsync(
            request.UserId, request.Month, request.Year,
            request.Type, request.CategoryId, request.MemberId,
            request.SearchTerm, request.PageNumber, request.PageSize);

        var mappedItems = paged.Items.Select(t => new TransactionResponse(
            t.Id,
            t.MemberId, t.Member.Name, t.Member.Color,
            t.CategoryId, t.Category.Name, t.Category.Icon, t.Category.Color,
            t.Type.ToString(), t.Amount, t.Note,
            t.TransactionDate, t.CreatedAt)).ToList().AsReadOnly();

        var result = new PaginatedList<TransactionResponse>(
            mappedItems, paged.TotalCount, paged.PageNumber,
            request.PageSize == 0 ? 1 : request.PageSize);

        return Result.Success(result);
    }
}
