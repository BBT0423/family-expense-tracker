using SummaryYearSalary.Domain.Entities;
using SummaryYearSalary.Domain.Enums;

namespace SummaryYearSalary.Application.Interfaces.IRepositories;

public interface ICategoryRepository : IRepository<Category>
{
    Task<IEnumerable<Category>> GetByUserIdAsync(string userId, TransactionType? type = null);
    Task<bool> HasTransactionsAsync(int categoryId);
}
