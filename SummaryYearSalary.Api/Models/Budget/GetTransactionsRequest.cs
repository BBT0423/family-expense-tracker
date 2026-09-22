namespace SummaryYearSalary.Api.Models.Budget;

public class GetTransactionsRequest
{
    public int? Month { get; init; }
    public int? Year { get; init; }
    public string? Type { get; init; }
    public int? CategoryId { get; init; }
    public string? SearchTerm { get; init; }
    public int? PageNumber { get; init; }
    public int? PageSize { get; init; }
    public string? UserId { get; init; }
}
