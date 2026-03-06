using SummaryYearSalary.Application.Interfaces.IRepositories;
using SummaryYearSalary.Domain.Common;
using SummaryYearSalary.Domain.Entities;
using SummaryYearSalary.Infrastructure.Data;

using Microsoft.EntityFrameworkCore;

namespace SummaryYearSalary.Infrastructure.Repositories;

public class RefreshTokenRepository(SummaryYearSalaryDbContext context) : IRefreshTokenRepository
{
    private readonly SummaryYearSalaryDbContext _context = context;
    public async Task AddAsync(RefreshToken refreshToken)
    {
        await _context.RefreshTokens.AddAsync(refreshToken);
        await _context.SaveChangesAsync();
    }

    public async Task<RefreshToken?> GetByTokenAsync(string token)
    {
        return await _context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == token);
    }

    public async Task RemoveAsync(RefreshToken refreshToken)
    {
        _context.RefreshTokens.Remove(refreshToken);
        await _context.SaveChangesAsync();
    }

    public async Task RemoveExpiredTokensAsync(string userId)
    {
        var expiredTokens = await _context.RefreshTokens
            .Where(rt => rt.UserId == userId && 
                         (rt.ExpiresAt < DateTimeProvider.GetBangkokTimeZone() || rt.RevokedAt != null))
            .ToListAsync();

        _context.RefreshTokens.RemoveRange(expiredTokens);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(RefreshToken token)
    {
        _context.RefreshTokens.Update(token);
        await _context.SaveChangesAsync();
    }
}