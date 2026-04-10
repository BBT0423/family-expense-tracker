using SummaryYearSalary.Application.Interfaces.IRepositories;
using SummaryYearSalary.Domain.Entities;
using SummaryYearSalary.Infrastructure.Data;

using Microsoft.EntityFrameworkCore;

namespace SummaryYearSalary.Infrastructure.Repositories;

public class FamilyMemberRepository(SummaryYearSalaryDbContext context)
    : Repository<FamilyMember>(context), IFamilyMemberRepository
{
    public async Task<IEnumerable<FamilyMember>> GetByUserIdAsync(string userId)
    {
        return await _dbSet
            .Where(m => m.UserId == userId && m.IsActive)
            .OrderBy(m => m.Name)
            .ToListAsync();
    }

    public async Task<bool> ExistsByUserIdAndNameAsync(string userId, string name, int? excludeId = null)
    {
        var query = _dbSet.Where(m => m.UserId == userId && m.Name == name);
        if (excludeId.HasValue)
            query = query.Where(m => m.Id != excludeId.Value);
        return await query.AnyAsync();
    }
}
