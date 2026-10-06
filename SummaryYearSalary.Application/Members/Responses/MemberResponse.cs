namespace SummaryYearSalary.Application.Members.Responses;

public record MemberResponse(
    int Id,
    string Name,
    string Color,
    bool IsActive);
