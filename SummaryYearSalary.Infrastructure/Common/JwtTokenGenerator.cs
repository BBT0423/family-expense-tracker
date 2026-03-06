using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

using SummaryYearSalary.Application.Interfaces;
using SummaryYearSalary.Domain.Common;
using SummaryYearSalary.Domain.Entities;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace SummaryYearSalary.Infrastructure.Common;

public class JwtTokenGenerator(
    UserManager<ApplicationUser> userManager,
    IOptions<JwtSettings> jwtSettings
) : IJwtTokenGenerator
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly JwtSettings _jwtSettings = jwtSettings.Value;
    
    public async Task<string> GenerateJwtTokenAsync(ApplicationUser user)
    {
        ArgumentNullException.ThrowIfNull(user);

        var roles = await _userManager.GetRolesAsync(user);
        
        var signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_jwtSettings.Secret)),
            SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id ?? string.Empty),
            new(ClaimTypes.Name, user.UserName ?? string.Empty),
            new(ClaimTypes.GivenName, $"{user.FirstName} {user.LastName}"),
            new(ClaimTypes.Email, user.Email ?? string.Empty)
        };


        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTimeProvider.GetBangkokTimeZone().AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
            signingCredentials: signingCredentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task<RefreshToken> GenerateRefreshTokenAsync(string userId, string ipAddress)
    {
        var refreshToken = new RefreshToken
        {
            UserId = userId,
            Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
            ExpiresAt = DateTimeProvider.GetBangkokTimeZone().AddDays(_jwtSettings.RefreshTokenExpirationDays),
            CreatedByIp = ipAddress
        };

        return refreshToken;
    }

    public async Task<RefreshToken> RotateRefreshTokenAsync(RefreshToken token, string ipAddress)
    {
        var newRefreshToken = await GenerateRefreshTokenAsync(token.UserId, ipAddress);
        token.RevokedAt = DateTimeProvider.GetBangkokTimeZone();
        token.RevokedByIp = ipAddress;
        token.ReplacedByToken = newRefreshToken.Token;
        token.ReasonRevoked = "Replaced by new token";

        return newRefreshToken;
    }
}