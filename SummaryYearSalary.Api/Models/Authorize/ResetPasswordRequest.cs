namespace SummaryYearSalary.Api.Models.Authorize;

/// <summary>
/// The response type for the "/resetPassword" endpoint.
/// The "/resetPassword" endpoint requires the "/forgotPassword" endpoint to be called first to get the <see cref="Token"/>.
/// </summary>
public class ResetPasswordRequest
{
    /// <summary>
    /// The email address for the user requesting a password reset. This should match <see cref="ForgotPasswordRequest.Email"/>.
    /// </summary>
    public required string Email { get; init; }

    /// <summary>
    /// The code sent to the user's email to reset the password. To get the reset code, first make a "/forgotPassword" request.
    /// </summary>
    public required string Token { get; init; }

    /// <summary>
    /// The new password the user with the given <see cref="Email"/> should login with. This will replace the previous password.
    /// </summary>
    public required string NewPassword { get; init; }
}