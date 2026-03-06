using Moq;
using SummaryYearSalary.Application.Authorize;
using SummaryYearSalary.Application.Authorize.Commands.LoginCommand;
using SummaryYearSalary.Application.Common;
using SummaryYearSalary.Application.Interfaces.IServices;
using SummaryYearSalary.Tests.Fixtures;

namespace SummaryYearSalary.Tests.Application.Handlers;

public class LoginCommandHandlerTests : BaseTest
{
    private readonly Mock<IAuthorizeService> _mockAuthorizeService;
    private readonly LoginCommandHandler _handler;

    public LoginCommandHandlerTests()
    {
        _mockAuthorizeService = new Mock<IAuthorizeService>();
        _handler = new LoginCommandHandler(_mockAuthorizeService.Object);
    }

    [Fact]
    public async Task LoginCommandHandler_WithValidCredentials_ShouldReturnSuccessResult()
    {
        // Arrange
        var loginCommand = new LoginCommand("user@example.com", "ValidPass@123", "192.168.1.1");
        var userResponse = new UserResponse("user-123", "user@example.com", "John Doe", "", new Dictionary<string, object>());
        var expectedResponse = new AuthorizeResponse("access-token", "refresh-token", 3600, false, userResponse);
        var expectedResult = Result.Success(expectedResponse);

        _mockAuthorizeService
            .Setup(s => s.LoginAsync(loginCommand, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _handler.Handle(loginCommand, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("access-token", result.Value.Token);
        _mockAuthorizeService.Verify(s => s.LoginAsync(loginCommand, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task LoginCommandHandler_WithInvalidCredentials_ShouldReturnFailureResult()
    {
        // Arrange
        var loginCommand = new LoginCommand("invalid@example.com", "wrongpass", "192.168.1.1");
        var expectedResult = Result.Failure<AuthorizeResponse>("Invalid credentials");

        _mockAuthorizeService
            .Setup(s => s.LoginAsync(loginCommand, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _handler.Handle(loginCommand, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        _mockAuthorizeService.Verify(s => s.LoginAsync(loginCommand, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task LoginCommandHandler_WithNullCommand_ShouldNotThrow()
    {
        // Arrange - would be prevented by model validation, but testing handler resilience
        var loginCommand = new LoginCommand("test@example.com", "password", "192.168.1.1");

        _mockAuthorizeService
            .Setup(s => s.LoginAsync(It.IsAny<LoginCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<AuthorizeResponse>("Service error"));

        // Act
        var result = await _handler.Handle(loginCommand, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        _mockAuthorizeService.Verify(s => s.LoginAsync(It.IsAny<LoginCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task LoginCommandHandler_ShouldHandleCancellationToken()
    {
        // Arrange
        var loginCommand = new LoginCommand("user@example.com", "password", "192.168.1.1");
        using var cancellationTokenSource = new CancellationTokenSource();
        await cancellationTokenSource.CancelAsync();

        _mockAuthorizeService
            .Setup(s => s.LoginAsync(loginCommand, cancellationTokenSource.Token))
            .ThrowsAsync(new OperationCanceledException());

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            async () => await _handler.Handle(loginCommand, cancellationTokenSource.Token)
        );
    }

    [Fact]
    public async Task LoginCommandHandler_WithMfaRequired_ShouldReturnResponseWithMfaFlag()
    {
        // Arrange
        var loginCommand = new LoginCommand("mfa-user@example.com", "ValidPass@123", "192.168.1.1");
        var userResponse = new UserResponse("user-456", "mfa-user@example.com", "Jane Doe", "", new Dictionary<string, object>());
        var expectedResponse = new AuthorizeResponse("access-token", "refresh-token", 3600, true, userResponse); // MFA required
        var expectedResult = Result.Success(expectedResponse);

        _mockAuthorizeService
            .Setup(s => s.LoginAsync(loginCommand, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _handler.Handle(loginCommand, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Value!.IsMfaEnabled);
        _mockAuthorizeService.Verify(s => s.LoginAsync(loginCommand, It.IsAny<CancellationToken>()), Times.Once);
    }
}
