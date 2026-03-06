using SummaryYearSalary.Infrastructure.Services;

namespace SummaryYearSalary.Tests.Infrastructure.Services;

public class OtpServiceTests
{
    private readonly OtpService _otpService = new();

    [Fact]
    public void GenerateSecret_ShouldReturnNonEmptyString()
    {
        // Act
        var secret = _otpService.GenerateSecret();

        // Assert
        Assert.NotNull(secret);
        Assert.NotEmpty(secret);
    }

    [Fact]
    public void GenerateSecret_ShouldReturnBase32EncodedString()
    {
        // Act
        var secret = _otpService.GenerateSecret();

        // Assert
        // Base32 should only contain A-Z and 2-7
        Assert.True(secret.All(c => (c >= 'A' && c <= 'Z') || (c >= '2' && c <= '7')));
    }

    [Fact]
    public void GenerateSecret_MultipleCallsShouldReturnDifferentSecrets()
    {
        // Act
        var secret1 = _otpService.GenerateSecret();
        var secret2 = _otpService.GenerateSecret();

        // Assert
        Assert.NotEqual(secret1, secret2);
    }

    [Fact]
    public void GenerateTotp_WithValidSecret_ShouldReturnSixDigitCode()
    {
        // Arrange
        var secret = _otpService.GenerateSecret();

        // Act
        var code = _otpService.GenerateTotp(secret);

        // Assert
        Assert.NotNull(code);
        Assert.Equal(6, code.Length);
        Assert.True(code.All(char.IsDigit));
    }

    [Fact]
    public void GenerateTotp_ShouldGenerateConsistentCodeForTimeWindow()
    {
        // Arrange
        var secret = _otpService.GenerateSecret();

        // Act
        var code1 = _otpService.GenerateTotp(secret);
        var code2 = _otpService.GenerateTotp(secret);

        // Assert - Should be same within 30-second window
        Assert.Equal(code1, code2);
    }

    [Fact]
    public void VerifyTotp_WithCorrectCode_ShouldReturnTrue()
    {
        // Arrange
        var secret = _otpService.GenerateSecret();
        var correctCode = _otpService.GenerateTotp(secret);

        // Act
        var result = _otpService.VerifyTotp(secret, correctCode);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void VerifyTotp_WithIncorrectCode_ShouldReturnFalse()
    {
        // Arrange
        var secret = _otpService.GenerateSecret();
        var incorrectCode = "000000";

        // Act
        var result = _otpService.VerifyTotp(secret, incorrectCode);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void VerifyTotp_WithEmptySecret_ShouldReturnFalse()
    {
        // Arrange
        var secret = string.Empty;
        var code = "123456";

        // Act
        var result = _otpService.VerifyTotp(secret, code);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void VerifyTotp_WithEmptyCode_ShouldReturnFalse()
    {
        // Arrange
        var secret = _otpService.GenerateSecret();
        var code = string.Empty;

        // Act
        var result = _otpService.VerifyTotp(secret, code);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void VerifyTotp_WithNullSecret_ShouldReturnFalse()
    {
        // Arrange
        string secret = null!;
        var code = "123456";

        // Act
        var result = _otpService.VerifyTotp(secret, code);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void VerifyTotp_WithNullCode_ShouldReturnFalse()
    {
        // Arrange
        var secret = _otpService.GenerateSecret();
        string code = null!;

        // Act
        var result = _otpService.VerifyTotp(secret, code);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void VerifyTotp_WithInvalidBase32Secret_ShouldReturnFalse()
    {
        // Arrange
        var invalidSecret = "INVALID@#$%";
        var code = "123456";

        // Act
        var result = _otpService.VerifyTotp(invalidSecret, code);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GenerateQrCodeUri_ShouldContainEmailAndSecret()
    {
        // Arrange
        var email = "user@example.com";
        var secret = _otpService.GenerateSecret();

        // Act
        var uri = _otpService.GenerateQrCodeUri(email, secret);

        // Assert
        Assert.NotNull(uri);
        Assert.Contains("otpauth://totp", uri);
        Assert.Contains(Uri.EscapeDataString(email), uri);
        Assert.Contains(secret, uri);
    }

    [Fact]
    public void GenerateQrCodeUri_WithCustomIssuer_ShouldIncludeIssuer()
    {
        // Arrange
        var email = "user@example.com";
        var secret = _otpService.GenerateSecret();
        var customIssuer = "CustomApp";

        // Act
        var uri = _otpService.GenerateQrCodeUri(email, secret, customIssuer);

        // Assert
        Assert.Contains(customIssuer, uri);
        Assert.Contains("issuer=CustomApp", uri);
    }

    [Fact]
    public void GenerateQrCodeUri_ShouldBeValidUri()
    {
        // Arrange
        var email = "user@example.com";
        var secret = _otpService.GenerateSecret();

        // Act
        var uri = _otpService.GenerateQrCodeUri(email, secret);

        // Assert
        Assert.True(Uri.TryCreate(uri, UriKind.Absolute, out _));
    }

    [Fact]
    public void GenerateQrCodeUri_WithSpecialCharactersInEmail_ShouldEscapeProper()
    {
        // Arrange
        var email = "user+tag@example.com";
        var secret = _otpService.GenerateSecret();

        // Act
        var uri = _otpService.GenerateQrCodeUri(email, secret);

        // Assert
        Assert.Contains(Uri.EscapeDataString(email), uri);
        // Should be properly URL encoded
        Assert.True(Uri.TryCreate(uri, UriKind.Absolute, out _));
    }
}
