using Microsoft.AspNetCore.Authentication.BearerToken;

namespace SummaryYearSalary.Api.Models.Authorize;


/// <summary>
/// The request type for the "/refresh" endpoint.
/// </summary>
public class RefreshTokenRequest
{
    /// <summary>
    /// The <see cref="AccessTokenResponse.RefreshToken"/> from the last "/login" or "/refresh" response used to get a new <see cref="AccessTokenResponse"/>
    /// with an extended expiration.
    /// </summary>
    public required string RefreshToken { get; init; }
}
