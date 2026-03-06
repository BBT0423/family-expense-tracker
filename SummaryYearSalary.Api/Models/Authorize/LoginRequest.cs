namespace SummaryYearSalary.Api.Models.Authorize;


/// <summary>
/// The request type for the "/login" endpoint.
/// </summary>
public class LoginRequest
{
    /// <summary>
    /// The user's email address which acts as a user name.
    /// </summary>
    public required string Email { get; init; }

    /// <summary>
    /// The user's password.
    /// </summary>
    public required string Password { get; init; }
}