namespace SummaryYearSalary.Api.Models.Budget;

public class BulkImportItemRequest
{
    public int CategoryId { get; init; }
    public int? MemberId { get; init; }
    public required string Type { get; init; }
    public decimal Amount { get; init; }
    public string? Note { get; init; }
    public DateTime TransactionDate { get; init; }
}

public class BulkImportRequest
{
    public required IEnumerable<BulkImportItemRequest> Items { get; init; }
}
