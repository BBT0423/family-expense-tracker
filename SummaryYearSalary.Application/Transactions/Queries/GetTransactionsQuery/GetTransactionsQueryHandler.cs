using SummaryYearSalary.Application.Common;
using SummaryYearSalary.Application.Interfaces.IRepositories;
using SummaryYearSalary.Application.Transactions.Responses;
using SummaryYearSalary.Domain.Entities;

using MediatR;

using Microsoft.AspNetCore.Identity;

namespace SummaryYearSalary.Application.Transactions.Queries.GetTransactionsQuery;

public class GetTransactionsQueryHandler(
    ITransactionRepository transactionRepository,
    UserManager<ApplicationUser> userManager
) : IRequestHandler<GetTransactionsQuery, Result<PaginatedList<TransactionResponse>>>
{
    private readonly ITransactionRepository _transactionRepository = transactionRepository;
    private readonly UserManager<ApplicationUser> _userManager = userManager;

    public async Task<Result<PaginatedList<TransactionResponse>>> Handle(GetTransactionsQuery request, CancellationToken cancellationToken)
    {
        var paged = await ReportScope.ResolvePagedAsync(
            _transactionRepository, request.UserId, request.ScopeUserId,
            request.Month, request.Year, request.Type, request.CategoryId,
            request.SearchTerm, request.PageNumber, request.PageSize, request.MemberId);

        var combined = ReportScope.IsCombined(request.UserId, request.ScopeUserId);

        var nameCache = new Dictionary<string, string?>();
        async Task<string?> ResolveUserNameAsync(string userId)
        {
            if (nameCache.TryGetValue(userId, out var cached))
                return cached;

            var name = UserDisplayName.Of(await _userManager.FindByIdAsync(userId));
            nameCache[userId] = name;
            return name;
        }

        var mappedItems = new List<TransactionResponse>();
        foreach (var t in paged.Items)
        {
            string? createdByName = t.CreatedByUserId != t.UserId
                ? await ResolveUserNameAsync(t.CreatedByUserId)
                : null;
            string? ownerName = combined ? await ResolveUserNameAsync(t.UserId) : null;

            mappedItems.Add(new TransactionResponse(
                t.Id,
                t.CategoryId, t.Category.Name, t.Category.Icon, t.Category.Color,
                t.Type.ToString(), t.Amount, t.Note,
                t.TransactionDate, t.CreatedAt, createdByName, ownerName,
                t.MemberId, t.Member?.Name, t.Member?.Color));
        }

        var result = new PaginatedList<TransactionResponse>(
            mappedItems.AsReadOnly(), paged.TotalCount, paged.PageNumber,
            request.PageSize == 0 ? 1 : request.PageSize);

        return Result.Success(result);
    }
}
