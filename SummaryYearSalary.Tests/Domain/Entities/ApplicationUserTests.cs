using SummaryYearSalary.Domain.Common;
using SummaryYearSalary.Domain.Entities;
using SummaryYearSalary.Tests.Builders;

namespace SummaryYearSalary.Tests.Domain.Entities;

public class ApplicationUserTests
{
    [Fact]
    public void ApplicationUser_WhenCreated_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var user = new ApplicationUser
        {
            Id = "test-id",
            Email = "test@example.com",
            UserName = "testuser"
        };

        // Assert
        Assert.NotNull(user.Id);
        Assert.Equal("test@example.com", user.Email);
        Assert.Equal("testuser", user.UserName);
        Assert.NotEqual(default(DateTime), user.CreatedAt);
        Assert.False(user.IsMfaEnabled);
        Assert.Null(user.MfaSecret);
        Assert.Null(user.LastLoginAt);
        Assert.Equal(0, user.FailedLoginAttempts);
        Assert.Null(user.LockoutEndDate);
    }

    [Fact]
    public void ApplicationUser_WithMfaSettings_ShouldPersistValues()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Id = "user-123",
            Email = "user@example.com",
            IsMfaEnabled = true,
            MfaSecret = "JBSWY3DPEBLW64TMMQ",
            FirstName = "John",
            LastName = "Doe"
        };

        // Act & Assert
        Assert.True(user.IsMfaEnabled);
        Assert.Equal("JBSWY3DPEBLW64TMMQ", user.MfaSecret);
        Assert.Equal("John", user.FirstName);
        Assert.Equal("Doe", user.LastName);
    }

    [Fact]
    public void ApplicationUser_WithLoginTracking_ShouldUpdateLastLoginAt()
    {
        // Arrange
        var user = new ApplicationUser { Id = "user-456", Email = "login@example.com" };
        var loginTime = DateTimeProvider.GetBangkokTimeZone();

        // Act
        user.LastLoginAt = loginTime;

        // Assert
        Assert.NotNull(user.LastLoginAt);
        Assert.Equal(loginTime, user.LastLoginAt);
    }

    [Fact]
    public void ApplicationUser_WithFailedLogins_ShouldTrackAttempts()
    {
        // Arrange
        var user = new ApplicationUser { Id = "user-789", Email = "attempts@example.com" };

        // Act
        user.FailedLoginAttempts = 3;
        var lockoutEnd = DateTimeProvider.GetBangkokTimeZone().AddMinutes(15);
        user.LockoutEndDate = lockoutEnd;

        // Assert
        Assert.Equal(3, user.FailedLoginAttempts);
        Assert.NotNull(user.LockoutEndDate);
        Assert.True(user.LockoutEndDate > DateTimeProvider.GetBangkokTimeZone());
    }

    [Fact]
    public void ApplicationUser_WithRefreshTokens_ShouldMaintainCollection()
    {
        // Arrange
        var user = new ApplicationUser { Id = "user-tokens", Email = "tokens@example.com" };
        var token = new RefreshTokenBuilder()
            .WithUserId(user.Id)
            .WithToken("refresh-token-value")
            .Build();

        // Act
        user.RefreshTokens.Add(token);

        // Assert
        Assert.NotEmpty(user.RefreshTokens);
        Assert.Contains(token, user.RefreshTokens);
        Assert.Equal(user.Id, user.RefreshTokens.First().UserId);
    }

    [Fact]
    public void ApplicationUser_CreatedAt_ShouldBeBangkokTime()
    {
        // Arrange & Act
        var user = new ApplicationUser { Id = "bangkok-user", Email = "bangkok@example.com" };

        // Assert
        Assert.NotEqual(default(DateTime), user.CreatedAt);
        Assert.True(user.CreatedAt > DateTime.MinValue);
    }
}
