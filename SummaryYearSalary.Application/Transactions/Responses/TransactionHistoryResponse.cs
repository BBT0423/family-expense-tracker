namespace SummaryYearSalary.Application.Transactions.Responses;

public record TransactionHistoryResponse(
    int Id,
    string FieldName,
    string? OldValue,
    string? NewValue,
    string ChangedByName,
    DateTime ChangedAt);
