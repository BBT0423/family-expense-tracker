using Moq;
using SummaryYearSalary.Application.Common;
using SummaryYearSalary.Application.Interfaces.IServices;
using SummaryYearSalary.Application.User.Commands.ConfirmMfaCommand;
using SummaryYearSalary.Tests.Fixtures;

namespace SummaryYearSalary.Tests.Application.Handlers;

public class ConfirmMfaCommandHandlerTests : BaseTest
{
    private readonly Mock<IAuthorizeService> _mockAuthorizeService = new();
    private readonly ConfirmMfaCommandHandler _handler;

    public ConfirmMfaCommandHandlerTests()
    {
        _handler = new ConfirmMfaCommandHandler(_mockAuthorizeService.Object);
    }

    [Fact]
    public async Task Handle_WithValidCode_ReturnsSuccess()
    {
        var command = new ConfirmMfaCommand("user-123", "123456", "127.0.0.1");
        _mockAuthorizeService
            .Setup(s => s.ConfirmMfaAsync("user-123", "123456", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(true));

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        _mockAuthorizeService.Verify(s => s.ConfirmMfaAsync("user-123", "123456", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithInvalidCode_ReturnsFailure()
    {
        var command = new ConfirmMfaCommand("user-123", "000000", "127.0.0.1");
        _mockAuthorizeService
            .Setup(s => s.ConfirmMfaAsync("user-123", "000000", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<bool>("Invalid verification code."));

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("Invalid verification code.", result.Error);
    }

    [Theory]
    [InlineData("")]
    [InlineData("12345")]
    [InlineData("1234567")]
    [InlineData("12a456")]
    public void Validator_RejectsCodesThatAreNotSixDigits(string code)
    {
        var validator = new ConfirmMfaCommandValidator();

        var result = validator.Validate(new ConfirmMfaCommand("user-123", code, "127.0.0.1"));

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validator_AcceptsSixDigitCode()
    {
        var validator = new ConfirmMfaCommandValidator();

        var result = validator.Validate(new ConfirmMfaCommand("user-123", "123456", "127.0.0.1"));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void ConfirmMfaCommand_IsMarkedSensitive()
    {
        Assert.NotNull(Attribute.GetCustomAttribute(typeof(ConfirmMfaCommand), typeof(SensitiveRequestAttribute)));
    }
}
