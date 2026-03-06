using SummaryYearSalary.Domain.Entities;

namespace SummaryYearSalary.Application.Interfaces.IRepositories;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenAsync(string token);
    Task AddAsync(RefreshToken refreshToken);
    Task RemoveAsync(RefreshToken refreshToken);
    Task RemoveExpiredTokensAsync(string userId);
    Task UpdateAsync(RefreshToken token);
}