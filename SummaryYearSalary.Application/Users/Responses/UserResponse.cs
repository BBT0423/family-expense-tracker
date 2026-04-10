namespace SummaryYearSalary.Application.Users.Responses;

public record UserResponse(
    string Id,
    string? FirstName,
    string? LastName,
    string? Email,
    string? PhoneNumber,
    DateTime CreatedAt,
    DateTime? LastLoginAt,
    IList<string> Roles);
