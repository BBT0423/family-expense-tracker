using Moq;
using SummaryYearSalary.Application.Authorize;
using SummaryYearSalary.Application.Authorize.Queries.VerifyMfaQuery;
using SummaryYearSalary.Application.Common;
using SummaryYearSalary.Application.Interfaces.IServices;
using SummaryYearSalary.Tests.Fixtures;

namespace SummaryYearSalary.Tests.Application.Handlers;

public class VerifyMfaQueryHandlerTests : BaseTest
{
    private readonly Mock<IAuthorizeService> _mockAuthorizeService;
    private readonly VerifyMfaQueryHandler _handler;

    public VerifyMfaQueryHandlerTests()
    {
        _mockAuthorizeService = new Mock<IAuthorizeService>();
        _handler = new VerifyMfaQueryHandler(_mockAuthorizeService.Object);
    }

    [Fact]
    public async Task VerifyMfaQueryHandler_WithValidMfaCode_ShouldReturnSuccessResult()
    {
        // Arrange
        var query = new VerifyMfaQuery("user-123", "123456", "192.168.1.1");
        var userResponse = new UserResponse("user-123", "user@example.com", "John Doe", "", new Dictionary<string, object>());
        var expectedResponse = new AuthorizeResponse("access-token", "refresh-token", 3600, false, userResponse);
        var expectedResult = Result.Success(expectedResponse);

        _mockAuthorizeService
            .Setup(s => s.VerifyMfaAsync(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("access-token", result.Value.Token);
        _mockAuthorizeService.Verify(s => s.VerifyMfaAsync(query, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task VerifyMfaQueryHandler_WithInvalidMfaCode_ShouldReturnFailure()
    {
        // Arrange
        var query = new VerifyMfaQuery("user-123", "000000", "192.168.1.1");
        var expectedResult = Result.Failure<AuthorizeResponse>("Invalid MFA code");

        _mockAuthorizeService
            .Setup(s => s.VerifyMfaAsync(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        _mockAuthorizeService.Verify(s => s.VerifyMfaAsync(query, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task VerifyMfaQueryHandler_WithUserNotFound_ShouldReturnFailure()
    {
        // Arrange
        var query = new VerifyMfaQuery("non-existent-user", "123456", "192.168.1.1");
        var expectedResult = Result.Failure<AuthorizeResponse>("User not found");

        _mockAuthorizeService
            .Setup(s => s.VerifyMfaAsync(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        _mockAuthorizeService.Verify(s => s.VerifyMfaAsync(query, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task VerifyMfaQueryHandler_ShouldParameterizeUserIdAndMfaCode()
    {
        // Arrange
        var userId = "specific-user";
        var mfaCode = "654321";
        var ipAddress = "10.0.0.1";
        var query = new VerifyMfaQuery(userId, mfaCode, ipAddress);
        var userResponse = new UserResponse(userId, "user@example.com", "Test User", "", new Dictionary<string, object>());

        _mockAuthorizeService
            .Setup(s => s.VerifyMfaAsync(It.IsAny<VerifyMfaQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(new AuthorizeResponse("token", "refresh", 3600, false, userResponse)));

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert - Verify service was called with correct query parameters
        _mockAuthorizeService.Verify(
            s => s.VerifyMfaAsync(
                It.Is<VerifyMfaQuery>(q => 
                    q.UserId == userId && 
                    q.MfaCode == mfaCode && 
                    q.IpAddress == ipAddress
                ),
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task VerifyMfaQueryHandler_ShouldHandleCancellation()
    {
        // Arrange
        var query = new VerifyMfaQuery("user-123", "123456", "192.168.1.1");
        using var cancellationTokenSource = new CancellationTokenSource();
        await cancellationTokenSource.CancelAsync();

        _mockAuthorizeService
            .Setup(s => s.VerifyMfaAsync(query, cancellationTokenSource.Token))
            .ThrowsAsync(new OperationCanceledException());

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            async () => await _handler.Handle(query, cancellationTokenSource.Token)
        );
    }
}
