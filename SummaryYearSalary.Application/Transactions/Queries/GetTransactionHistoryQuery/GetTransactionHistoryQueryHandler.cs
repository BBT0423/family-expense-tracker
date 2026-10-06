using SummaryYearSalary.Application.Common;
using SummaryYearSalary.Application.Interfaces.IRepositories;
using SummaryYearSalary.Application.Transactions.Responses;
using SummaryYearSalary.Domain.Entities;

using MediatR;

using Microsoft.AspNetCore.Identity;

namespace SummaryYearSalary.Application.Transactions.Queries.GetTransactionHistoryQuery;

public class GetTransactionHistoryQueryHandler(
    ITransactionRepository transactionRepository,
    ITransactionHistoryRepository transactionHistoryRepository,
    UserManager<ApplicationUser> userManager
) : IRequestHandler<GetTransactionHistoryQuery, Result<List<TransactionHistoryResponse>>>
{
    private readonly ITransactionRepository _transactionRepository = transactionRepository;
    private readonly ITransactionHistoryRepository _transactionHistoryRepository = transactionHistoryRepository;
    private readonly UserManager<ApplicationUser> _userManager = userManager;

    public async Task<Result<List<TransactionHistoryResponse>>> Handle(GetTransactionHistoryQuery request, CancellationToken cancellationToken)
    {
        var transaction = await _transactionRepository.GetByIdAsync(request.TransactionId);
        if (transaction is null || transaction.UserId != request.UserId)
            return Result.Failure<List<TransactionHistoryResponse>>("ไม่พบรายการที่ต้องการ", 404);

        var history = await _transactionHistoryRepository.GetByTransactionIdAsync(request.TransactionId);

        var nameCache = new Dictionary<string, string>();
        async Task<string> ResolveUserNameAsync(string userId)
        {
            if (nameCache.TryGetValue(userId, out var cached))
                return cached;

            var name = UserDisplayName.Of(await _userManager.FindByIdAsync(userId));
            nameCache[userId] = name;
            return name;
        }

        var items = new List<TransactionHistoryResponse>();
        foreach (var h in history)
        {
            items.Add(new TransactionHistoryResponse(
                h.Id, h.FieldName, h.OldValue, h.NewValue,
                await ResolveUserNameAsync(h.ChangedByUserId), h.CreatedAt));
        }

        return Result.Success(items);
    }
}
