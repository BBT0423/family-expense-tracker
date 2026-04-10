using SummaryYearSalary.Application.Interfaces.IRepositories;
using SummaryYearSalary.Domain.Entities;
using SummaryYearSalary.Infrastructure.Data;

using Microsoft.EntityFrameworkCore;

namespace SummaryYearSalary.Infrastructure.Repositories;

public class BudgetRepository(SummaryYearSalaryDbContext context)
    : Repository<Budget>(context), IBudgetRepository
{
    public async Task<IEnumerable<Budget>> GetByUserMonthYearAsync(string userId, int month, int year)
    {
        return await _dbSet
            .Include(b => b.Category)
            .Where(b => b.UserId == userId && b.Month == month && b.Year == year)
            .ToListAsync();
    }

    public async Task<Budget?> GetByUserCategoryMonthYearAsync(string userId, int categoryId, int month, int year)
    {
        return await _dbSet
            .FirstOrDefaultAsync(b => b.UserId == userId
                && b.CategoryId == categoryId
                && b.Month == month
                && b.Year == year);
    }
}
