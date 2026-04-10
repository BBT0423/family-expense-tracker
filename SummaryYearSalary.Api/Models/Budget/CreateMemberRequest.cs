namespace SummaryYearSalary.Api.Models.Budget;

public class CreateMemberRequest
{
    public required string Name { get; init; }
    public required string Color { get; init; }
    public string? Avatar { get; init; }
}
