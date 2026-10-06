namespace SummaryYearSalary.Application.Transactions.Responses;

public record TransactionResponse(
    int Id,
    int CategoryId,
    string CategoryName,
    string CategoryIcon,
    string CategoryColor,
    string Type,
    decimal Amount,
    string? Note,
    DateTime TransactionDate,
    DateTime CreatedAt,
    string? CreatedByName = null,
    string? OwnerName = null,
    int? MemberId = null,
    string? MemberName = null,
    string? MemberColor = null);
