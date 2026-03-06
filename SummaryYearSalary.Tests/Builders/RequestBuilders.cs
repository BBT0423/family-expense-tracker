using SummaryYearSalary.Api.Models.Authorize;
using SummaryYearSalary.Api.Models.User;

namespace SummaryYearSalary.Tests.Builders;

/// <summary>
/// Builder for creating LoginRequest test data with fluent API.
/// Simplifies test setup and makes tests more readable.
/// </summary>
public class LoginRequestBuilder
{
    private string _email = "test@example.com";
    private string _password = "TestPassword@123";

    /// <summary>
    /// Set the email for the login request.
    /// </summary>
    public LoginRequestBuilder WithEmail(string email)
    {
        _email = email;
        return this;
    }

    /// <summary>
    /// Set the password for the login request.
    /// </summary>
    public LoginRequestBuilder WithPassword(string password)
    {
        _password = password;
        return this;
    }

    /// <summary>
    /// Build and return the LoginRequest.
    /// </summary>
    public LoginRequest Build()
    {
        return new LoginRequest
        {
            Email = _email,
            Password = _password
        };
    }
}

/// <summary>
/// Builder for creating RefreshTokenRequest test data.
/// </summary>
public class RefreshTokenRequestBuilder
{
    private string _refreshToken = "valid-refresh-token-123";

    public RefreshTokenRequestBuilder WithToken(string token)
    {
        _refreshToken = token;
        return this;
    }

    public RefreshTokenRequest Build()
    {
        return new RefreshTokenRequest
        {
            RefreshToken = _refreshToken
        };
    }
}

/// <summary>
/// Builder for creating ForgotPasswordRequest test data.
/// </summary>
public class ForgotPasswordRequestBuilder
{
    private string _email = "user@example.com";

    public ForgotPasswordRequestBuilder WithEmail(string email)
    {
        _email = email;
        return this;
    }

    public ForgotPasswordRequest Build()
    {
        return new ForgotPasswordRequest
        {
            Email = _email
        };
    }
}

/// <summary>
/// Builder for creating ResetPasswordRequest test data.
/// </summary>
public class ResetPasswordRequestBuilder
{
    private string _email = "user@example.com";
    private string _token = "reset-token-abc123";
    private string _newPassword = "NewPassword@456";

    public ResetPasswordRequestBuilder WithEmail(string email)
    {
        _email = email;
        return this;
    }

    public ResetPasswordRequestBuilder WithToken(string token)
    {
        _token = token;
        return this;
    }

    public ResetPasswordRequestBuilder WithNewPassword(string password)
    {
        _newPassword = password;
        return this;
    }

    public ResetPasswordRequest Build()
    {
        return new ResetPasswordRequest
        {
            Email = _email,
            Token = _token,
            NewPassword = _newPassword
        };
    }
}

/// <summary>
/// Builder for creating VerifyMfaRequest test data.
/// </summary>
public class VerifyMfaRequestBuilder
{
    private string _userId = "user-123";
    private string _mfaCode = "123456";

    public VerifyMfaRequestBuilder WithUserId(string userId)
    {
        _userId = userId;
        return this;
    }

    public VerifyMfaRequestBuilder WithMfaCode(string code)
    {
        _mfaCode = code;
        return this;
    }

    public VerifyMfaRequest Build()
    {
        return new VerifyMfaRequest
        {
            UserId = _userId,
            MfaCode = _mfaCode
        };
    }
}

/// <summary>
/// Builder for creating ChangePasswordRequest test data.
/// </summary>
public class ChangePasswordRequestBuilder
{
    private string _userId = "user-123";
    private string _currentPassword = "CurrentPass@123";
    private string _newPassword = "NewPass@456";
    private string _confirmPassword = "NewPass@456";

    public ChangePasswordRequestBuilder WithUserId(string userId)
    {
        _userId = userId;
        return this;
    }

    public ChangePasswordRequestBuilder WithCurrentPassword(string password)
    {
        _currentPassword = password;
        return this;
    }

    public ChangePasswordRequestBuilder WithNewPassword(string password)
    {
        _newPassword = password;
        _confirmPassword = password; // Keep in sync by default
        return this;
    }

    public ChangePasswordRequestBuilder WithConfirmPassword(string password)
    {
        _confirmPassword = password;
        return this;
    }

    public ChangePasswordRequest Build()
    {
        return new ChangePasswordRequest
        {
            UserId = _userId,
            CurrentPassword = _currentPassword,
            NewPassword = _newPassword,
            ConfirmPassword = _confirmPassword
        };
    }
}
