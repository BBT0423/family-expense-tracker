namespace SummaryYearSalary.Api.Models.Budget;

public class UpsertBudgetRequest
{
    public required int CategoryId { get; init; }
    public required int Month { get; init; }
    public required int Year { get; init; }
    public required decimal LimitAmount { get; init; }
}
