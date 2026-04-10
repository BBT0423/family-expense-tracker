namespace SummaryYearSalary.Api.Models.Budget;

public class UpdateTransactionRequest
{
    public required int MemberId { get; init; }
    public required int CategoryId { get; init; }
    public required string Type { get; init; }
    public required decimal Amount { get; init; }
    public string? Note { get; init; }
    public required DateTime TransactionDate { get; init; }
}
