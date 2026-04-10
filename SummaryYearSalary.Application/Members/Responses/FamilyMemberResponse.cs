namespace SummaryYearSalary.Application.Members.Responses;

public record FamilyMemberResponse(
    int Id,
    string Name,
    string Color,
    string? Avatar,
    bool IsActive,
    DateTime CreatedAt);
