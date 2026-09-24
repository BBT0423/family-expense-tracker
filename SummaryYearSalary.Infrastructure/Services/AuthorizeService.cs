using System.Net.Mail;
using System.Text;

using SummaryYearSalary.Application.Authorize;
using SummaryYearSalary.Application.Authorize.Commands.ForgotPasswordCommand;
using SummaryYearSalary.Application.Authorize.Commands.LoginCommand;
using SummaryYearSalary.Application.Authorize.Commands.RefreshTokenCommand;
using SummaryYearSalary.Application.Authorize.Commands.ResetPasswordCommand;
using SummaryYearSalary.Application.Authorize.Queries.VerifyMfaQuery;
using SummaryYearSalary.Application.Common;
using SummaryYearSalary.Application.Interfaces;
using SummaryYearSalary.Domain.Common;
using SummaryYearSalary.Application.Interfaces.IRepositories;
using SummaryYearSalary.Application.Interfaces.IServices;
using SummaryYearSalary.Application.User;
using SummaryYearSalary.Application.User.Commands.ChangePasswordCommand;
using SummaryYearSalary.Domain.Entities;
using SummaryYearSalary.Infrastructure.Common;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace SummaryYearSalary.Infrastructure.Services;

// <summary>
// Implementation of the IAuthorizeService interface for handling user authorization.
// </summary>
public class AuthorizeService(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    IOtpService otpService,
    IRefreshTokenRepository refreshTokenRepository,
    IEmailRepository emailRepository,
    IJwtTokenGenerator jwtTokenGenerator,
    IOptions<AuthorizeServiceSettings> settings
) : IAuthorizeService
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly SignInManager<ApplicationUser> _signInManager = signInManager;
    private readonly IOtpService _otpService = otpService;
    private readonly IRefreshTokenRepository _refreshTokenRepository = refreshTokenRepository;
    private readonly IEmailRepository _emailRepository = emailRepository;
    private readonly IJwtTokenGenerator _jwtTokenGenerator = jwtTokenGenerator;
    private readonly SecuritySettings _securitySettings = settings.Value.Security;
    private readonly JwtSettings _jwtSettings = settings.Value.Jwt;
    private readonly EmailSettings _emailSettings = settings.Value.Email;

    public async Task<Result<bool>> ChangePasswordAsync(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user == null)
        {
            return Result.Failure<bool>("User not found.");
        }

        var changeResult = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!changeResult.Succeeded)
        {
            var errors = string.Join("; ", changeResult.Errors.Select(e => e.Description));
            return Result.Failure<bool>($"Password change failed: {errors}");
        }

        var body = new StringBuilder(request.Template);
        body.Replace("[Name]", user.FirstName);
        using var message = new MailMessage(new MailAddress(user.Email!), new MailAddress(user.Email!))
        {
            Subject = "_SummaryYearSalary - Your Password Has Been Changed",
            Body = body.ToString()
        };

        await _emailRepository.SendEmailAsync(message, cancellationToken);
        return Result.Success<bool>(true);
    }

    public async Task<Result<EnableMfaResponse>> EnableMfaAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return Result.Failure<EnableMfaResponse>("User not found.");
        }

        if (user.IsMfaEnabled)
        {
            return Result.Failure<EnableMfaResponse>("MFA is already enabled for this user.");
        }

        var mfaSecret = _otpService.GenerateSecret();
        var mfaAuthenticatorUrl = _otpService.GenerateQrCodeUri(user.Email ?? string.Empty, mfaSecret, _jwtSettings.Audience);
        // The secret is only stored here; MFA is switched on by ConfirmMfaAsync once the user proves
        // their authenticator app works. Otherwise abandoning setup would lock them out of login.
        user.MfaSecret = mfaSecret;
        await _userManager.UpdateAsync(user);
        return Result.Success(new EnableMfaResponse(mfaSecret, mfaAuthenticatorUrl));
    }

    public async Task<Result<bool>> ConfirmMfaAsync(string userId, string code, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return Result.Failure<bool>("User not found.");
        }

        if (user.IsMfaEnabled)
        {
            return Result.Failure<bool>("MFA is already enabled for this user.");
        }

        if (string.IsNullOrEmpty(user.MfaSecret))
        {
            return Result.Failure<bool>("MFA setup has not been started.");
        }

        if (!_otpService.VerifyTotp(user.MfaSecret, code))
        {
            return Result.Failure<bool>("Invalid verification code.");
        }

        user.IsMfaEnabled = true;
        await _userManager.UpdateAsync(user);
        return Result.Success(true);
    }

    public async Task<Result<bool>> ForgotPasswordAsync(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            return Result.Failure<bool>("We couldn't find a user with that email address. Please check the email address and try again.");
        }

        var body = new StringBuilder(request.Template);
        var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
        var resetLink = $"{_emailSettings.Web}/authorize/reset-password?token={Uri.EscapeDataString(resetToken)}";

        body.Replace("[Name]", user.FirstName);
        body.Replace("[ResetPasswordLink]", resetLink);

        using var message = new MailMessage(new MailAddress(request.Email), new MailAddress(request.Email))
        {
            Subject = "_SummaryYearSalary - Reset Your Password",
            Body = body.ToString()
        };
        await _emailRepository.SendEmailAsync(message, cancellationToken);
        return Result.Success<bool>(true);
    }

    public async Task<Result<AuthorizeResponse>> LoginAsync(LoginCommand request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            return Result.Unauthorized<AuthorizeResponse>("Invalid login attempt.");
        }

        if (user.LockoutEndDate.HasValue && user.LockoutEndDate > DateTimeProvider.GetBangkokTimeZone())
        {
            return Result.Unauthorized<AuthorizeResponse>($"Account is locked until {user.LockoutEndDate}");
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: false);
        if (!result.Succeeded)
        {
            user.FailedLoginAttempts++;

            if (user.FailedLoginAttempts >= _securitySettings.MaxFailedLoginAttempts)
            {
                user.LockoutEndDate = DateTimeProvider.GetBangkokTimeZone().AddMinutes(_securitySettings.LockoutDurationMinutes);
                await _userManager.UpdateAsync(user);
                return Result.Unauthorized<AuthorizeResponse>("Account locked due to multiple failed login attempts");
            }

            await _userManager.UpdateAsync(user);
            return Result.Unauthorized<AuthorizeResponse>("Invalid credentials");
        }

        user.FailedLoginAttempts = 0;
        user.LockoutEndDate = null;
        user.LastLoginAt = DateTimeProvider.GetBangkokTimeZone();
        await _userManager.UpdateAsync(user);

        var metadata = await BuildMetadata(user);

        if (user.IsMfaEnabled)
        {
            return Result.Success(new AuthorizeResponse(
                string.Empty,
                string.Empty,
                _jwtSettings.AccessTokenExpirationMinutes,
                user.IsMfaEnabled,
                new UserResponse(user.Id ?? string.Empty, user.Email ?? string.Empty, $"{user.FirstName} {user.LastName}", string.Empty, metadata)
            ));
        }

        var accessToken = await _jwtTokenGenerator.GenerateJwtTokenAsync(user);
        var refreshToken = await _jwtTokenGenerator.GenerateRefreshTokenAsync(user.Id!, request.IpAddress);
        await _refreshTokenRepository.AddAsync(refreshToken);

        return Result.Success(new AuthorizeResponse(
            accessToken,
            refreshToken.Token,
            _jwtSettings.AccessTokenExpirationMinutes,
            user.IsMfaEnabled,
            new UserResponse(user.Id ?? string.Empty, user.Email ?? string.Empty, $"{user.FirstName} {user.LastName}", string.Empty, metadata)
        ));
    }

    public async Task<Result<AuthorizeResponse>> RefreshTokenAsync(RefreshTokenCommand request, CancellationToken cancellationToken = default)
    {
        var token = await _refreshTokenRepository.GetByTokenAsync(request.RefreshToken);
        if (token == null || token.IsRevoked || token.ExpiresAt < DateTimeProvider.GetBangkokTimeZone())
        {
            return Result.Unauthorized<AuthorizeResponse>("Invalid or expired refresh token.");
        }

        var newRefreshToken = await _jwtTokenGenerator.RotateRefreshTokenAsync(token, request.IpAddress);
        var accessToken = await _jwtTokenGenerator.GenerateJwtTokenAsync(token.User!);
        await _refreshTokenRepository.UpdateAsync(newRefreshToken);

        var metadata = await BuildMetadata(token.User!);

        return Result.Success(new AuthorizeResponse(
            accessToken,
            newRefreshToken.Token,
            _jwtSettings.AccessTokenExpirationMinutes,
            token.User!.IsMfaEnabled,
            new UserResponse(token.User!.Id ?? string.Empty, token.User.Email ?? string.Empty, $"{token.User.FirstName} {token.User.LastName}", string.Empty, metadata)
        ));
    }

    public async Task<Result<bool>> ResetPasswordAsync(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            return Result.Failure<bool>("We couldn't find a user with that email address. Please check the email address and try again.");
        }

        var body = new StringBuilder(request.Template);
        var resetResult = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
        if (!resetResult.Succeeded)
        {
            var errors = string.Join("; ", resetResult.Errors.Select(e => e.Description));
            return Result.Failure<bool>($"Password reset failed: {errors}");
        }

        body.Replace("[Name]", user.FirstName);
        using var message = new MailMessage(new MailAddress(request.Email), new MailAddress(request.Email))
        {
            Subject = "_SummaryYearSalary - Your Password Has Been Reset",
            Body = body.ToString()
        };

        await _emailRepository.SendEmailAsync(message, cancellationToken);
        return Result.Success<bool>(true);
    }

    public async Task<Result<AuthorizeResponse>> GoogleLoginAsync(string email, string name, string googleId, string ipAddress, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(email);

        if (user == null)
        {
            var nameParts = name.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            user = new ApplicationUser
            {
                Id = Guid.NewGuid().ToString(),
                Email = email,
                UserName = email,
                FirstName = nameParts.Length > 0 ? nameParts[0] : name,
                LastName = nameParts.Length > 1 ? nameParts[1] : string.Empty,
                EmailConfirmed = true,
                CreatedAt = DateTimeProvider.GetBangkokTimeZone()
            };

            var createResult = await _userManager.CreateAsync(user);
            if (!createResult.Succeeded)
            {
                var errors = string.Join("; ", createResult.Errors.Select(e => e.Description));
                return Result.Failure<AuthorizeResponse>($"Failed to create user: {errors}");
            }

            await _userManager.AddToRoleAsync(user, "User");
        }

        var loginInfo = new UserLoginInfo("Google", googleId, "Google");
        var existingLogins = await _userManager.GetLoginsAsync(user);
        if (!existingLogins.Any(l => l.LoginProvider == "Google" && l.ProviderKey == googleId))
        {
            await _userManager.AddLoginAsync(user, loginInfo);
        }

        user.LastLoginAt = DateTimeProvider.GetBangkokTimeZone();
        await _userManager.UpdateAsync(user);

        var accessToken = await _jwtTokenGenerator.GenerateJwtTokenAsync(user);
        var refreshToken = await _jwtTokenGenerator.GenerateRefreshTokenAsync(user.Id!, ipAddress);
        await _refreshTokenRepository.AddAsync(refreshToken);

        return Result.Success(new AuthorizeResponse(
            accessToken,
            refreshToken.Token,
            _jwtSettings.AccessTokenExpirationMinutes,
            user.IsMfaEnabled,
            new UserResponse(user.Id ?? string.Empty, user.Email ?? string.Empty, $"{user.FirstName} {user.LastName}".Trim(), string.Empty, await BuildMetadata(user))
        ));
    }

    public async Task<Result<AuthorizeResponse>> RegisterAsync(string email, string password, string firstName, string lastName, string ipAddress, CancellationToken cancellationToken = default)
    {
        var existingUser = await _userManager.FindByEmailAsync(email);
        if (existingUser != null)
        {
            return Result.Failure<AuthorizeResponse>("An account with this email address already exists.");
        }

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            Email = email,
            UserName = email,
            FirstName = firstName,
            LastName = lastName,
            EmailConfirmed = true,
            CreatedAt = DateTimeProvider.GetBangkokTimeZone()
        };

        var createResult = await _userManager.CreateAsync(user, password);
        if (!createResult.Succeeded)
        {
            var errors = string.Join("; ", createResult.Errors.Select(e => e.Description));
            return Result.Failure<AuthorizeResponse>($"Registration failed: {errors}");
        }

        await _userManager.AddToRoleAsync(user, "User");

        var accessToken = await _jwtTokenGenerator.GenerateJwtTokenAsync(user);
        var refreshToken = await _jwtTokenGenerator.GenerateRefreshTokenAsync(user.Id!, ipAddress);
        await _refreshTokenRepository.AddAsync(refreshToken);

        return Result.Success(new AuthorizeResponse(
            accessToken,
            refreshToken.Token,
            _jwtSettings.AccessTokenExpirationMinutes,
            user.IsMfaEnabled,
            new UserResponse(user.Id ?? string.Empty, user.Email ?? string.Empty, $"{user.FirstName} {user.LastName}".Trim(), string.Empty, await BuildMetadata(user))
        ));
    }

    public async Task<Result<AuthorizeResponse>> VerifyMfaAsync(VerifyMfaQuery request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user == null || string.IsNullOrEmpty(user.MfaSecret))
        {
            return Result.Unauthorized<AuthorizeResponse>("Invalid user or MFA not enabled.");
        }

        var isCodeValid = _otpService.VerifyTotp(user.MfaSecret, request.MfaCode);
        if (!isCodeValid)
        {
            return Result.Unauthorized<AuthorizeResponse>("Invalid MFA code.");
        }

        var accessToken = await _jwtTokenGenerator.GenerateJwtTokenAsync(user);
        var refreshToken = await _jwtTokenGenerator.GenerateRefreshTokenAsync(user.Id, request.IpAddress);
        await _refreshTokenRepository.AddAsync(refreshToken);

        return Result.Success(new AuthorizeResponse(
            accessToken,
            refreshToken.Token,
            _jwtSettings.AccessTokenExpirationMinutes,
            user.IsMfaEnabled,
            new UserResponse(user.Id ?? string.Empty, user.Email ?? string.Empty, $"{user.FirstName} {user.LastName}", string.Empty, await BuildMetadata(user))
        ));
    }

    private async Task<Dictionary<string, object>> BuildMetadata(ApplicationUser user) => new()
    {
        { "userId", user.Id ?? string.Empty },
        { "firstName", user.FirstName ?? string.Empty },
        { "lastName", user.LastName ?? string.Empty },
        { "userName", user.UserName ?? string.Empty },
        { "isMfaEnabled", user.IsMfaEnabled },
        { "emailConfirmed", user.EmailConfirmed },
        { "createdAt", user.CreatedAt.ToString("o") },
        { "lastLogin", user.LastLoginAt?.ToString("o") ?? string.Empty },
        { "roles", await _userManager.GetRolesAsync(user) }
    };
}
