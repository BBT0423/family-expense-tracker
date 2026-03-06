using SummaryYearSalary.Domain.Common;
using SummaryYearSalary.Domain.Entities;

namespace SummaryYearSalary.Tests.Builders;

public class ApplicationUserBuilder
{
    private string _id = Guid.NewGuid().ToString();
    private string? _email = "user@example.com";
    private string? _userName = "testuser";
    private string? _firstName = "John";
    private string? _lastName = "Doe";
    private bool _isMfaEnabled = false;
    private string? _mfaSecret = null;
    private int _failedLoginAttempts = 0;
    private DateTime? _lockoutEndDate = null;
    private DateTime? _lastLoginAt = null;

    public ApplicationUserBuilder WithId(string id)
    {
        _id = id;
        return this;
    }

    public ApplicationUserBuilder WithEmail(string email)
    {
        _email = email;
        return this;
    }

    public ApplicationUserBuilder WithUserName(string userName)
    {
        _userName = userName;
        return this;
    }

    public ApplicationUserBuilder WithFirstName(string firstName)
    {
        _firstName = firstName;
        return this;
    }

    public ApplicationUserBuilder WithLastName(string lastName)
    {
        _lastName = lastName;
        return this;
    }

    public ApplicationUserBuilder WithMfaEnabled(bool isMfaEnabled)
    {
        _isMfaEnabled = isMfaEnabled;
        return this;
    }

    public ApplicationUserBuilder WithMfaSecret(string mfaSecret)
    {
        _mfaSecret = mfaSecret;
        _isMfaEnabled = true;
        return this;
    }

    public ApplicationUserBuilder WithFailedLoginAttempts(int attempts)
    {
        _failedLoginAttempts = attempts;
        return this;
    }

    public ApplicationUserBuilder WithLockoutEndDate(DateTime lockoutEndDate)
    {
        _lockoutEndDate = lockoutEndDate;
        return this;
    }

    public ApplicationUserBuilder WithLastLoginAt(DateTime lastLoginAt)
    {
        _lastLoginAt = lastLoginAt;
        return this;
    }

    public ApplicationUser Build()
    {
        return new ApplicationUser
        {
            Id = _id,
            Email = _email,
            UserName = _userName,
            FirstName = _firstName,
            LastName = _lastName,
            IsMfaEnabled = _isMfaEnabled,
            MfaSecret = _mfaSecret,
            FailedLoginAttempts = _failedLoginAttempts,
            LockoutEndDate = _lockoutEndDate,
            LastLoginAt = _lastLoginAt
        };
    }
}

public class RefreshTokenBuilder
{
    private int _id = 1;
    private string _userId = Guid.NewGuid().ToString();
    private string _token = "refresh-token-value";
    private DateTime _expiresAt = DateTimeProvider.GetBangkokTimeZone().AddDays(7);
    private string _createdByIp = "127.0.0.1";
    private DateTime? _revokedAt = null;
    private string? _revokedByIp = null;
    private string? _replacedByToken = null;
    private string? _reasonRevoked = null;

    public RefreshTokenBuilder WithId(int id)
    {
        _id = id;
        return this;
    }

    public RefreshTokenBuilder WithUserId(string userId)
    {
        _userId = userId;
        return this;
    }

    public RefreshTokenBuilder WithToken(string token)
    {
        _token = token;
        return this;
    }

    public RefreshTokenBuilder WithExpiresAt(DateTime expiresAt)
    {
        _expiresAt = expiresAt;
        return this;
    }

    public RefreshTokenBuilder WithCreatedByIp(string ip)
    {
        _createdByIp = ip;
        return this;
    }

    public RefreshTokenBuilder WithRevokedAt(DateTime revokedAt)
    {
        _revokedAt = revokedAt;
        return this;
    }

    public RefreshTokenBuilder WithRevokedByIp(string ip)
    {
        _revokedByIp = ip;
        return this;
    }

    public RefreshTokenBuilder WithReplacedByToken(string token)
    {
        _replacedByToken = token;
        return this;
    }

    public RefreshTokenBuilder WithReasonRevoked(string reason)
    {
        _reasonRevoked = reason;
        return this;
    }

    public RefreshToken Build()
    {
        return new RefreshToken
        {
            Id = _id,
            UserId = _userId,
            Token = _token,
            ExpiresAt = _expiresAt,
            CreatedByIp = _createdByIp,
            RevokedAt = _revokedAt,
            RevokedByIp = _revokedByIp,
            ReplacedByToken = _replacedByToken,
            ReasonRevoked = _reasonRevoked
        };
    }
}
