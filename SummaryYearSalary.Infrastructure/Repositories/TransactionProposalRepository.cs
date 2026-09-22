using SummaryYearSalary.Application.Interfaces.IRepositories;
using SummaryYearSalary.Domain.Entities;
using SummaryYearSalary.Domain.Enums;
using SummaryYearSalary.Infrastructure.Data;

using Microsoft.EntityFrameworkCore;

namespace SummaryYearSalary.Infrastructure.Repositories;

public class TransactionProposalRepository(SummaryYearSalaryDbContext context)
    : Repository<TransactionProposal>(context), ITransactionProposalRepository
{
    public async Task<IEnumerable<TransactionProposal>> GetPendingByOwnerAsync(string ownerUserId)
    {
        return await _dbSet
            .Include(p => p.Category)
            .Where(p => p.OwnerUserId == ownerUserId && p.Status == TransactionProposalStatus.Pending)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }
}
