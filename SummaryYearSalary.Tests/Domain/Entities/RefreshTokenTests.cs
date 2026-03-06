using SummaryYearSalary.Domain.Common;
using SummaryYearSalary.Domain.Entities;
using SummaryYearSalary.Tests.Builders;

namespace SummaryYearSalary.Tests.Domain.Entities;

public class RefreshTokenTests
{
    [Fact]
    public void RefreshToken_WhenCreated_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var token = new RefreshTokenBuilder()
            .WithToken("test-token")
            .Build();

        // Assert
        Assert.True(token.Id > 0);
        Assert.NotEmpty(token.Token);
        Assert.False(token.IsExpired);
        Assert.False(token.IsRevoked);
        Assert.True(token.IsActive);
    }

    [Fact]
    public void RefreshToken_WhenExpired_IsExpiredShouldReturnTrue()
    {
        // Arrange
        var pastDate = DateTimeProvider.GetBangkokTimeZone().AddHours(-1);
        var token = new RefreshTokenBuilder()
            .WithExpiresAt(pastDate)
            .Build();

        // Act & Assert
        Assert.True(token.IsExpired);
    }

    [Fact]
    public void RefreshToken_WhenNotExpired_IsExpiredShouldReturnFalse()
    {
        // Arrange
        var futureDate = DateTimeProvider.GetBangkokTimeZone().AddDays(7);
        var token = new RefreshTokenBuilder()
            .WithExpiresAt(futureDate)
            .Build();

        // Act & Assert
        Assert.False(token.IsExpired);
    }

    [Fact]
    public void RefreshToken_WhenRevoked_IsRevokedShouldReturnTrue()
    {
        // Arrange
        var revokedDate = DateTimeProvider.GetBangkokTimeZone();
        var token = new RefreshTokenBuilder()
            .WithRevokedAt(revokedDate)
            .Build();

        // Act & Assert
        Assert.True(token.IsRevoked);
    }

    [Fact]
    public void RefreshToken_WhenNotRevoked_IsRevokedShouldReturnFalse()
    {
        // Arrange
        var token = new RefreshTokenBuilder().Build();

        // Act & Assert
        Assert.False(token.IsRevoked);
    }

    [Fact]
    public void RefreshToken_WhenActiveAndNotExpired_IsActiveShouldReturnTrue()
    {
        // Arrange
        var futureDate = DateTimeProvider.GetBangkokTimeZone().AddDays(7);
        var token = new RefreshTokenBuilder()
            .WithExpiresAt(futureDate)
            .Build();

        // Act & Assert
        Assert.True(token.IsActive);
        Assert.False(token.IsRevoked);
        Assert.False(token.IsExpired);
    }

    [Fact]
    public void RefreshToken_WhenRevoked_IsActiveShouldReturnFalse()
    {
        // Arrange
        var futureDate = DateTimeProvider.GetBangkokTimeZone().AddDays(7);
        var token = new RefreshTokenBuilder()
            .WithExpiresAt(futureDate)
            .WithRevokedAt(DateTimeProvider.GetBangkokTimeZone())
            .Build();

        // Act & Assert
        Assert.False(token.IsActive);
    }

    [Fact]
    public void RefreshToken_WhenExpired_IsActiveShouldReturnFalse()
    {
        // Arrange
        var pastDate = DateTimeProvider.GetBangkokTimeZone().AddHours(-1);
        var token = new RefreshTokenBuilder()
            .WithExpiresAt(pastDate)
            .Build();

        // Act & Assert
        Assert.False(token.IsActive);
    }

    [Fact]
    public void RefreshToken_WithUserIdAndToken_ShouldMaintainValues()
    {
        // Arrange
        var userId = "user-123";
        var tokenValue = "refresh-token-value";

        // Act
        var token = new RefreshTokenBuilder()
            .WithUserId(userId)
            .WithToken(tokenValue)
            .Build();

        // Assert
        Assert.Equal(userId, token.UserId);
        Assert.Equal(tokenValue, token.Token);
    }

    [Fact]
    public void RefreshToken_WithCreatedByIp_ShouldStoreIpAddress()
    {
        // Arrange
        var ipAddress = "192.168.1.100";

        // Act
        var token = new RefreshTokenBuilder()
            .WithCreatedByIp(ipAddress)
            .Build();

        // Assert
        Assert.Equal(ipAddress, token.CreatedByIp);
    }

    [Fact]
    public void RefreshToken_WhenRevokedWithReason_ShouldStoreRevocationInfo()
    {
        // Arrange
        var revokedDate = DateTimeProvider.GetBangkokTimeZone();
        var revokedByIp = "192.168.1.50";
        var reason = "User logged out";

        // Act
        var token = new RefreshTokenBuilder()
            .WithRevokedAt(revokedDate)
            .WithRevokedByIp(revokedByIp)
            .WithReasonRevoked(reason)
            .Build();

        // Assert
        Assert.NotNull(token.RevokedAt);
        Assert.Equal(revokedByIp, token.RevokedByIp);
        Assert.Equal(reason, token.ReasonRevoked);
    }

    [Fact]
    public void RefreshToken_WithReplacedByToken_ShouldStoreReplacementInfo()
    {
        // Arrange
        var newToken = "new-refresh-token";

        // Act
        var token = new RefreshTokenBuilder()
            .WithReplacedByToken(newToken)
            .Build();

        // Assert
        Assert.Equal(newToken, token.ReplacedByToken);
    }

    [Fact]
    public void RefreshToken_BoundaryTest_ExpiryAtExactMoment_ShouldBeExpired()
    {
        // Arrange
        var currentTime = DateTimeProvider.GetBangkokTimeZone();
        var token = new RefreshTokenBuilder()
            .WithExpiresAt(currentTime)
            .Build();

        // Act & Assert - Token expires at exact current time
        Assert.True(token.IsExpired);
    }
}
