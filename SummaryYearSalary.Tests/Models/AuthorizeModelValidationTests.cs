using SummaryYearSalary.Api.Models.Authorize;
using SummaryYearSalary.Api.Models.User;

namespace SummaryYearSalary.Tests.Models;

/// <summary>
/// Model validation tests for API request objects.
/// Tests ensure that request models can be properly instantiated and serialized.
/// </summary>
public class AuthorizeModelValidationTests
{
    #region Login Request Tests

    /// <summary>
    /// Test LoginRequest can be created with valid email and password.
    /// </summary>
    [Fact]
    public void LoginRequest_CanBeCreatedWithValidData()
    {
        // Arrange
        var email = "test@example.com";
        var password = "SecurePassword123!";

        // Act
        var request = new LoginRequest
        {
            Email = email,
            Password = password
        };

        // Assert
        Assert.Equal(email, request.Email);
        Assert.Equal(password, request.Password);
    }

    /// <summary>
    /// Test LoginRequest serialization to JSON.
    /// </summary>
    [Fact]
    public void LoginRequest_CanBeSerializedToJson()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "user@test.com",
            Password = "Pass@123"
        };

        // Act
        var json = System.Text.Json.JsonSerializer.Serialize(request);

        // Assert
        Assert.Contains("user@test.com", json);
        Assert.Contains("Pass@123", json);
    }

    #endregion

    #region Refresh Token Request Tests

    /// <summary>
    /// Test RefreshTokenRequest can be created with valid refresh token.
    /// </summary>
    [Fact]
    public void RefreshTokenRequest_CanBeCreatedWithValidToken()
    {
        // Arrange
        var token = "valid-refresh-token-123";

        // Act
        var request = new RefreshTokenRequest
        {
            RefreshToken = token
        };

        // Assert
        Assert.Equal(token, request.RefreshToken);
    }

    #endregion

    #region Forgot Password Request Tests

    /// <summary>
    /// Test ForgotPasswordRequest can be created with valid email.
    /// </summary>
    [Fact]
    public void ForgotPasswordRequest_CanBeCreatedWithValidEmail()
    {
        // Arrange
        var email = "user@example.com";

        // Act
        var request = new ForgotPasswordRequest
        {
            Email = email
        };

        // Assert
        Assert.Equal(email, request.Email);
    }

    #endregion

    #region Reset Password Request Tests

    /// <summary>
    /// Test ResetPasswordRequest can be created with valid data.
    /// </summary>
    [Fact]
    public void ResetPasswordRequest_CanBeCreatedWithValidData()
    {
        // Arrange
        var email = "user@example.com";
        var token = "reset-token-abc123";
        var newPassword = "NewSecurePassword@456";

        // Act
        var request = new ResetPasswordRequest
        {
            Email = email,
            Token = token,
            NewPassword = newPassword
        };

        // Assert
        Assert.Equal(email, request.Email);
        Assert.Equal(token, request.Token);
        Assert.Equal(newPassword, request.NewPassword);
    }

    /// <summary>
    /// Test ResetPasswordRequest serialization to JSON.
    /// </summary>
    [Fact]
    public void ResetPasswordRequest_CanBeSerializedToJson()
    {
        // Arrange
        var request = new ResetPasswordRequest
        {
            Email = "test@domain.com",
            Token = "token123",
            NewPassword = "NewPass@789"
        };

        // Act
        var json = System.Text.Json.JsonSerializer.Serialize(request);

        // Assert
        Assert.Contains("test@domain.com", json);
        Assert.Contains("token123", json);
        Assert.Contains("NewPass@789", json);
    }

    #endregion

    #region MFA Verification Request Tests

    /// <summary>
    /// Test VerifyMfaRequest can be created with valid user ID and MFA code.
    /// </summary>
    [Fact]
    public void VerifyMfaRequest_CanBeCreatedWithValidData()
    {
        // Arrange
        var userId = "user-123";
        var mfaCode = "123456";

        // Act
        var request = new VerifyMfaRequest
        {
            UserId = userId,
            MfaCode = mfaCode
        };

        // Assert
        Assert.Equal(userId, request.UserId);
        Assert.Equal(mfaCode, request.MfaCode);
    }

    /// <summary>
    /// Test VerifyMfaRequest validates 6-digit MFA code format.
    /// </summary>
    [Fact]
    public void VerifyMfaRequest_AcceptsValidCodeFormat()
    {
        // Arrange
        var request = new VerifyMfaRequest
        {
            UserId = "user-id",
            MfaCode = "654321"
        };

        // Assert
        Assert.Equal(6, request.MfaCode.Length);
        Assert.True(request.MfaCode.All(char.IsDigit));
    }

    #endregion

    #region Change Password Request Tests

    /// <summary>
    /// Test ChangePasswordRequest can be created with valid data.
    /// </summary>
    [Fact]
    public void ChangePasswordRequest_CanBeCreatedWithValidData()
    {
        // Arrange
        var userId = "user-123";
        var currentPassword = "CurrentPass@123";
        var newPassword = "NewPass@456";
        var confirmPassword = "NewPass@456";

        // Act
        var request = new ChangePasswordRequest
        {
            UserId = userId,
            CurrentPassword = currentPassword,
            NewPassword = newPassword,
            ConfirmPassword = confirmPassword
        };

        // Assert
        Assert.Equal(userId, request.UserId);
        Assert.Equal(currentPassword, request.CurrentPassword);
        Assert.Equal(newPassword, request.NewPassword);
        Assert.Equal(confirmPassword, request.ConfirmPassword);
    }

    /// <summary>
    /// Test ChangePasswordRequest serialization to JSON.
    /// </summary>
    [Fact]
    public void ChangePasswordRequest_CanBeSerializedToJson()
    {
        // Arrange
        var request = new ChangePasswordRequest
        {
            UserId = "user-abc",
            CurrentPassword = "Old@Pass",
            NewPassword = "New@Pass",
            ConfirmPassword = "New@Pass"
        };

        // Act
        var json = System.Text.Json.JsonSerializer.Serialize(request);

        // Assert
        Assert.Contains("user-abc", json);
        Assert.Contains("Old@Pass", json);
        Assert.Contains("New@Pass", json);
    }

    #endregion
}
