namespace SummaryYearSalary.Api.Models.Budget;

public class UpdateMemberRequest
{
    public required string Name { get; init; }
    public required string Color { get; init; }
    public required bool IsActive { get; init; }
}
