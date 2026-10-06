using SummaryYearSalary.Application.Interfaces.IRepositories;
using SummaryYearSalary.Domain.Entities;
using SummaryYearSalary.Infrastructure.Data;

using Microsoft.EntityFrameworkCore;

namespace SummaryYearSalary.Infrastructure.Repositories;

public class MemberRepository(SummaryYearSalaryDbContext context)
    : Repository<Member>(context), IMemberRepository
{
    public async Task<IEnumerable<Member>> GetAllActiveFirstAsync()
    {
        return await _dbSet.OrderBy(m => m.IsActive ? 0 : 1).ThenBy(m => m.Name).ToListAsync();
    }

    public async Task<bool> HasTransactionsAsync(int memberId)
    {
        return await _context.Transactions.AnyAsync(t => t.MemberId == memberId);
    }
}
