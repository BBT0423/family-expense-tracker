using SummaryYearSalary.Application.Authorize;
using SummaryYearSalary.Application.Authorize.Commands.ForgotPasswordCommand;
using SummaryYearSalary.Application.Authorize.Commands.LoginCommand;
using SummaryYearSalary.Application.Authorize.Commands.RefreshTokenCommand;
using SummaryYearSalary.Application.Authorize.Commands.ResetPasswordCommand;
using SummaryYearSalary.Application.Authorize.Commands.SendPhoneOtpCommand;
using SummaryYearSalary.Application.Authorize.Commands.VerifyPhoneOtpCommand;
using SummaryYearSalary.Application.Authorize.Queries.VerifyMfaQuery;
using SummaryYearSalary.Application.Common;
using SummaryYearSalary.Application.User;
using SummaryYearSalary.Application.User.Commands.ChangePasswordCommand;

namespace SummaryYearSalary.Application.Interfaces.IServices;

/// <summary>
/// Service interface for user authorization operations.
/// </summary>
public interface IAuthorizeService
{
    Task<Result<AuthorizeResponse>> LoginAsync(LoginCommand request, CancellationToken cancellationToken = default);
    Task<Result<AuthorizeResponse>> RefreshTokenAsync(RefreshTokenCommand request, CancellationToken cancellationToken = default);
    Task<Result<EnableMfaResponse>> EnableMfaAsync(string userId, CancellationToken cancellationToken = default);
    Task<Result<AuthorizeResponse>> VerifyMfaAsync(VerifyMfaQuery request, CancellationToken cancellationToken);
    Task<Result<bool>> ForgotPasswordAsync(ForgotPasswordCommand request, CancellationToken cancellationToken);
    Task<Result<bool>> ResetPasswordAsync(ResetPasswordCommand request, CancellationToken cancellationToken);
    Task<Result<bool>> ChangePasswordAsync(ChangePasswordCommand request, CancellationToken cancellationToken);
    Task<Result<bool>> SendPhoneOtpAsync(SendPhoneOtpCommand request, CancellationToken cancellationToken);
    Task<Result<AuthorizeResponse>> VerifyPhoneOtpAsync(VerifyPhoneOtpCommand request, CancellationToken cancellationToken);
    Task<Result<AuthorizeResponse>> GoogleLoginAsync(string email, string name, string googleId, string ipAddress, CancellationToken cancellationToken = default);
    Task<Result<AuthorizeResponse>> RegisterAsync(string email, string password, string firstName, string lastName, string ipAddress, CancellationToken cancellationToken = default);
}
