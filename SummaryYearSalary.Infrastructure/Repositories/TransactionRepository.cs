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
    public Task<PaginatedList<Transaction>> GetPagedByUserAsync(
        string userId, int month, int year,
        TransactionType? type, int? categoryId,
        string? searchTerm, int pageNumber, int pageSize, int? memberId = null) =>
        GetPagedAsync(userId, month, year, type, categoryId, searchTerm, pageNumber, pageSize, memberId);

    public Task<PaginatedList<Transaction>> GetAllUsersPagedAsync(
        int month, int year,
        TransactionType? type, int? categoryId,
        string? searchTerm, int pageNumber, int pageSize, int? memberId = null) =>
        GetPagedAsync(null, month, year, type, categoryId, searchTerm, pageNumber, pageSize, memberId);

    private async Task<PaginatedList<Transaction>> GetPagedAsync(
        string? userId, int month, int year,
        TransactionType? type, int? categoryId,
        string? searchTerm, int pageNumber, int pageSize, int? memberId)
    {
        var query = _dbSet
            .Include(t => t.Category)
            .Include(t => t.Member)
            .AsQueryable();

        if (userId is not null)
            query = query.Where(t => t.UserId == userId);

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

    public Task<IEnumerable<Transaction>> GetByUserMonthYearAsync(string userId, int month, int year, int? memberId = null) =>
        GetMonthYearAsync(userId, month, year, memberId);

    public Task<IEnumerable<Transaction>> GetAllUsersMonthYearAsync(int month, int year, int? memberId = null) =>
        GetMonthYearAsync(null, month, year, memberId);

    private async Task<IEnumerable<Transaction>> GetMonthYearAsync(string? userId, int month, int year, int? memberId)
    {
        var query = _dbSet.Include(t => t.Category).AsQueryable();

        if (month > 0)
            query = query.Where(t => t.TransactionDate.Month == month);
        if (year > 0)
            query = query.Where(t => t.TransactionDate.Year == year);
        if (userId is not null)
            query = query.Where(t => t.UserId == userId);
        if (memberId.HasValue)
            query = query.Where(t => t.MemberId == memberId.Value);

        return await query.OrderByDescending(t => t.TransactionDate).ToListAsync();
    }

    public Task<IEnumerable<Transaction>> GetByUserYearAsync(string userId, int year, int? memberId = null) =>
        GetYearAsync(userId, year, memberId);

    public Task<IEnumerable<Transaction>> GetAllUsersYearAsync(int year, int? memberId = null) =>
        GetYearAsync(null, year, memberId);

    private async Task<IEnumerable<Transaction>> GetYearAsync(string? userId, int year, int? memberId)
    {
        var query = _dbSet
            .Include(t => t.Category)
            .Where(t => t.TransactionDate.Year == year);

        if (userId is not null)
            query = query.Where(t => t.UserId == userId);
        if (memberId.HasValue)
            query = query.Where(t => t.MemberId == memberId.Value);

        return await query.OrderBy(t => t.TransactionDate).ToListAsync();
    }

    public async Task AddRangeAsync(IEnumerable<Transaction> transactions)
    {
        await _dbSet.AddRangeAsync(transactions);
    }
}
