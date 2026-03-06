using Moq;
using SummaryYearSalary.Application.Authorize;
using SummaryYearSalary.Application.Authorize.Commands.RefreshTokenCommand;
using SummaryYearSalary.Application.Common;
using SummaryYearSalary.Application.Interfaces.IServices;
using SummaryYearSalary.Tests.Fixtures;

namespace SummaryYearSalary.Tests.Application.Handlers;

public class RefreshTokenCommandHandlerTests : BaseTest
{
    private readonly Mock<IAuthorizeService> _mockAuthorizeService;
    private readonly RefreshTokenCommandHandler _handler;

    public RefreshTokenCommandHandlerTests()
    {
        _mockAuthorizeService = new Mock<IAuthorizeService>();
        _handler = new RefreshTokenCommandHandler(_mockAuthorizeService.Object);
    }

    [Fact]
    public async Task RefreshTokenCommandHandler_WithValidToken_ShouldReturnNewTokens()
    {
        // Arrange
        var refreshCommand = new RefreshTokenCommand("valid-refresh-token", "192.168.1.1");
        var userResponse = new UserResponse("user-789", "user@example.com", "John Doe", "", new Dictionary<string, object>());
        var expectedResponse = new AuthorizeResponse("new-access-token", "new-refresh-token", 3600, false, userResponse);
        var expectedResult = Result.Success(expectedResponse);

        _mockAuthorizeService
            .Setup(s => s.RefreshTokenAsync(refreshCommand, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _handler.Handle(refreshCommand, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("new-access-token", result.Value!.Token);
        Assert.Equal("new-refresh-token", result.Value.RefreshToken);
        _mockAuthorizeService.Verify(s => s.RefreshTokenAsync(refreshCommand, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RefreshTokenCommandHandler_WithExpiredToken_ShouldReturnUnauthorized()
    {
        // Arrange
        var refreshCommand = new RefreshTokenCommand("expired-refresh-token", "192.168.1.1");
        var expectedResult = Result.Failure<AuthorizeResponse>("Refresh token has expired");

        _mockAuthorizeService
            .Setup(s => s.RefreshTokenAsync(refreshCommand, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _handler.Handle(refreshCommand, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        _mockAuthorizeService.Verify(s => s.RefreshTokenAsync(refreshCommand, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RefreshTokenCommandHandler_WithInvalidToken_ShouldReturnFailure()
    {
        // Arrange
        var refreshCommand = new RefreshTokenCommand("invalid-token", "192.168.1.1");
        var expectedResult = Result.Failure<AuthorizeResponse>("Invalid refresh token");

        _mockAuthorizeService
            .Setup(s => s.RefreshTokenAsync(refreshCommand, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _handler.Handle(refreshCommand, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        _mockAuthorizeService.Verify(s => s.RefreshTokenAsync(refreshCommand, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RefreshTokenCommandHandler_ShouldDelegateToAuthorizationService()
    {
        // Arrange
        var refreshCommand = new RefreshTokenCommand("token", "192.168.1.1");
        _mockAuthorizeService
            .Setup(s => s.RefreshTokenAsync(It.IsAny<RefreshTokenCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<AuthorizeResponse>("Error"));

        // Act
        await _handler.Handle(refreshCommand, CancellationToken.None);

        // Assert - Verify service was called with correct command
        _mockAuthorizeService.Verify(
            s => s.RefreshTokenAsync(
                It.Is<RefreshTokenCommand>(cmd => cmd.RefreshToken == "token"),
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );
    }
}
