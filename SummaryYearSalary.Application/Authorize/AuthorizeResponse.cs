namespace SummaryYearSalary.Application.Authorize;

public sealed record AuthorizeResponse(
    string Token,
    string RefreshToken,
    int ExpiresIn,
    bool IsMfaEnabled,
    UserResponse User
);

public sealed record UserResponse(
    string Id,
    string Email,
    string Name,
    string Avatar,
    IDictionary<string, object> Metadata
);