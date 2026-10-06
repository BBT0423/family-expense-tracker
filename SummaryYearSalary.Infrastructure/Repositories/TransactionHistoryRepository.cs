using SummaryYearSalary.Application.Interfaces.IRepositories;
using SummaryYearSalary.Domain.Entities;
using SummaryYearSalary.Infrastructure.Data;

using Microsoft.EntityFrameworkCore;

namespace SummaryYearSalary.Infrastructure.Repositories;

public class TransactionHistoryRepository(SummaryYearSalaryDbContext context)
    : Repository<TransactionHistory>(context), ITransactionHistoryRepository
{
    public async Task<IEnumerable<TransactionHistory>> GetByTransactionIdAsync(int transactionId) =>
        await _dbSet
            .Where(h => h.TransactionId == transactionId)
            .OrderByDescending(h => h.CreatedAt)
            .ToListAsync();
}
