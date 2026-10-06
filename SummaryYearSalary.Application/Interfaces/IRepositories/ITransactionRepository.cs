using SummaryYearSalary.Application.Common;
using SummaryYearSalary.Domain.Entities;
using SummaryYearSalary.Domain.Enums;

namespace SummaryYearSalary.Application.Interfaces.IRepositories;

public interface ITransactionRepository : IRepository<Transaction>
{
    Task<PaginatedList<Transaction>> GetPagedByUserAsync(
        string userId, int month, int year,
        TransactionType? type, int? categoryId,
        string? searchTerm, int pageNumber, int pageSize, int? memberId = null);

    Task<IEnumerable<Transaction>> GetByUserMonthYearAsync(string userId, int month, int year, int? memberId = null);
    Task<IEnumerable<Transaction>> GetByUserYearAsync(string userId, int year, int? memberId = null);
    Task AddRangeAsync(IEnumerable<Transaction> transactions);

    // "All users" counterparts — used only for the combined household view (Dashboard).
    Task<PaginatedList<Transaction>> GetAllUsersPagedAsync(
        int month, int year,
        TransactionType? type, int? categoryId,
        string? searchTerm, int pageNumber, int pageSize, int? memberId = null);

    Task<IEnumerable<Transaction>> GetAllUsersMonthYearAsync(int month, int year, int? memberId = null);
    Task<IEnumerable<Transaction>> GetAllUsersYearAsync(int year, int? memberId = null);
}
