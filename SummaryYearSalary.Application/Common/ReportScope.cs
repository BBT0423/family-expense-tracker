using SummaryYearSalary.Application.Interfaces.IRepositories;
using SummaryYearSalary.Domain.Entities;
using SummaryYearSalary.Domain.Enums;

namespace SummaryYearSalary.Application.Common;

/// <summary>
/// Resolves the Reports/Dashboard "scope" a query should run against: the caller's own data
/// (default), one other specific user, or every user (Admin-only "household" combined view —
/// gated by the API layer, which only ever forwards a non-null scope for an Admin caller).
/// </summary>
public static class ReportScope
{
    public const string All = "all";

    public static Task<IEnumerable<Transaction>> ResolveMonthYearAsync(
        ITransactionRepository repository, string requesterUserId, string? scopeUserId, int month, int year) =>
        scopeUserId switch
        {
            All => repository.GetAllUsersMonthYearAsync(month, year),
            null or "" => repository.GetByUserMonthYearAsync(requesterUserId, month, year),
            _ => repository.GetByUserMonthYearAsync(scopeUserId, month, year)
        };

    public static Task<IEnumerable<Transaction>> ResolveYearAsync(
        ITransactionRepository repository, string requesterUserId, string? scopeUserId, int year) =>
        scopeUserId switch
        {
            All => repository.GetAllUsersYearAsync(year),
            null or "" => repository.GetByUserYearAsync(requesterUserId, year),
            _ => repository.GetByUserYearAsync(scopeUserId, year)
        };

    public static Task<PaginatedList<Transaction>> ResolvePagedAsync(
        ITransactionRepository repository, string requesterUserId, string? scopeUserId,
        int month, int year, TransactionType? type, int? categoryId,
        string? searchTerm, int pageNumber, int pageSize) =>
        scopeUserId switch
        {
            All => repository.GetAllUsersPagedAsync(month, year, type, categoryId, searchTerm, pageNumber, pageSize),
            null or "" => repository.GetPagedByUserAsync(requesterUserId, month, year, type, categoryId, searchTerm, pageNumber, pageSize),
            _ => repository.GetPagedByUserAsync(scopeUserId, month, year, type, categoryId, searchTerm, pageNumber, pageSize)
        };

    /// <summary>True once the results can include rows from more than one ApplicationUser — the
    /// point at which responses need an OwnerName to disambiguate whose account each row belongs to.</summary>
    public static bool IsCombined(string requesterUserId, string? scopeUserId) =>
        scopeUserId == All || (!string.IsNullOrEmpty(scopeUserId) && scopeUserId != requesterUserId);
}
