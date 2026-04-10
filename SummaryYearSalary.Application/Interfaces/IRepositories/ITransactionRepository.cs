using SummaryYearSalary.Application.Common;
using SummaryYearSalary.Domain.Entities;
using SummaryYearSalary.Domain.Enums;

namespace SummaryYearSalary.Application.Interfaces.IRepositories;

public interface ITransactionRepository : IRepository<Transaction>
{
    Task<PaginatedList<Transaction>> GetPagedByUserAsync(
        string userId, int month, int year,
        TransactionType? type, int? categoryId, int? memberId,
        string? searchTerm, int pageNumber, int pageSize);

    Task<IEnumerable<Transaction>> GetByUserMonthYearAsync(string userId, int month, int year);
    Task<IEnumerable<Transaction>> GetByUserYearAsync(string userId, int year);
    Task AddRangeAsync(IEnumerable<Transaction> transactions);
}
