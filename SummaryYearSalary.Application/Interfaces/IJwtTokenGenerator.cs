using SummaryYearSalary.Domain.Entities;

namespace SummaryYearSalary.Application.Interfaces;

public interface IJwtTokenGenerator
{
    Task<string> GenerateJwtTokenAsync(ApplicationUser user);
    Task<RefreshToken> GenerateRefreshTokenAsync(string userId, string ipAddress);
    Task<RefreshToken> RotateRefreshTokenAsync(RefreshToken token, string ipAddress);
}