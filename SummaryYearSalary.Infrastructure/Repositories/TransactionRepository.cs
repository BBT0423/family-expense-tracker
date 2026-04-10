using SummaryYearSalary.Application.Common;
using SummaryYearSalary.Application.Interfaces.IRepositories;
using SummaryYearSalary.Domain.Entities;
using SummaryYearSalary.Domain.Enums;
using SummaryYearSalary.Infrastructure.Data;

using Microsoft.EntityFrameworkCore;

namespace SummaryYearSalary.Infrastructure.Repositories;

public class TransactionRepository(SummaryYearSalaryDbContext context)
    : Repository<Transaction>(context), ITransactionRepository
{
    public async Task<PaginatedList<Transaction>> GetPagedByUserAsync(
        string userId, int month, int year,
        TransactionType? type, int? categoryId, int? memberId,
        string? searchTerm, int pageNumber, int pageSize)
    {
        var query = _dbSet
            .Include(t => t.Member)
            .Include(t => t.Category)
            .Where(t => t.UserId == userId);

        if (month > 0)
            query = query.Where(t => t.TransactionDate.Month == month);
        if (year > 0)
            query = query.Where(t => t.TransactionDate.Year == year);
        if (type.HasValue)
            query = query.Where(t => t.Type == type.Value);
        if (categoryId.HasValue)
            query = query.Where(t => t.CategoryId == categoryId.Value);
        if (memberId.HasValue)
            query = query.Where(t => t.MemberId == memberId.Value);
        if (!string.IsNullOrWhiteSpace(searchTerm))
            query = query.Where(t => t.Note != null && t.Note.Contains(searchTerm));

        var count = await query.CountAsync();
        var items = await query
            .OrderByDescending(t => t.TransactionDate)
            .ThenByDescending(t => t.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PaginatedList<Transaction>(items.AsReadOnly(), count, pageNumber, pageSize);
    }

    public async Task<IEnumerable<Transaction>> GetByUserMonthYearAsync(string userId, int month, int year)
    {
        return await _dbSet
            .Include(t => t.Member)
            .Include(t => t.Category)
            .Where(t => t.UserId == userId
                && t.TransactionDate.Month == month
                && t.TransactionDate.Year == year)
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Transaction>> GetByUserYearAsync(string userId, int year)
    {
        return await _dbSet
            .Include(t => t.Member)
            .Include(t => t.Category)
            .Where(t => t.UserId == userId && t.TransactionDate.Year == year)
            .OrderBy(t => t.TransactionDate)
            .ToListAsync();
    }

    public async Task AddRangeAsync(IEnumerable<Transaction> transactions)
    {
        await _dbSet.AddRangeAsync(transactions);
    }
}
