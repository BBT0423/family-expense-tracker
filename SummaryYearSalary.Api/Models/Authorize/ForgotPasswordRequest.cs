using System.ComponentModel.DataAnnotations;

namespace SummaryYearSalary.Api.Models.Authorize;

/// <summary>
/// The response type for the "/forgotPassword" endpoint.
/// </summary>
public sealed class ForgotPasswordRequest
{
    /// <summary>
    /// The email address to send the reset password code to if a user with that confirmed email address already exists.
    /// </summary>
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email address.")]
    public string Email { get; init; } = null!;
}