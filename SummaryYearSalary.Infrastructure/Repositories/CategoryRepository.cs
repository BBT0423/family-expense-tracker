using SummaryYearSalary.Application.Interfaces.IRepositories;
using SummaryYearSalary.Domain.Entities;
using SummaryYearSalary.Domain.Enums;
using SummaryYearSalary.Infrastructure.Data;

using Microsoft.EntityFrameworkCore;

namespace SummaryYearSalary.Infrastructure.Repositories;

public class CategoryRepository(SummaryYearSalaryDbContext context)
    : Repository<Category>(context), ICategoryRepository
{
    public async Task<IEnumerable<Category>> GetByUserIdAsync(string userId, TransactionType? type = null)
    {
        var query = _dbSet.Where(c => c.UserId == null || c.UserId == userId);

        if (type.HasValue)
            query = query.Where(c => c.Type == type.Value);

        return await query.OrderBy(c => c.IsDefault ? 0 : 1).ThenBy(c => c.Name).ToListAsync();
    }

    public async Task<bool> HasTransactionsAsync(int categoryId)
    {
        return await _context.Transactions.AnyAsync(t => t.CategoryId == categoryId);
    }
}
