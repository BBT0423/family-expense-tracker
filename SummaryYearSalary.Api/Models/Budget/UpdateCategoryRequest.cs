namespace SummaryYearSalary.Api.Models.Budget;

public class UpdateCategoryRequest
{
    public required string Name { get; init; }
    public required string Type { get; init; } // "Income" or "Expense"
    public required string Icon { get; init; }
    public required string Color { get; init; }
}
