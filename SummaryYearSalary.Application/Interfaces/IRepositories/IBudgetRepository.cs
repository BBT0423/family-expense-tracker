using SummaryYearSalary.Domain.Entities;

namespace SummaryYearSalary.Application.Interfaces.IRepositories;

public interface IBudgetRepository : IRepository<Budget>
{
    Task<IEnumerable<Budget>> GetByUserMonthYearAsync(string userId, int month, int year);
    Task<Budget?> GetByUserCategoryMonthYearAsync(string userId, int categoryId, int month, int year);
}
