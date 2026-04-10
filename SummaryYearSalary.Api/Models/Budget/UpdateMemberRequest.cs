namespace SummaryYearSalary.Api.Models.Budget;

public class UpdateMemberRequest
{
    public required string Name { get; init; }
    public required string Color { get; init; }
    public string? Avatar { get; init; }
    public bool IsActive { get; init; } = true;
}
