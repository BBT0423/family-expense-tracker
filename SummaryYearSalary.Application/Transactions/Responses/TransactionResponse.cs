namespace SummaryYearSalary.Application.Transactions.Responses;

public record TransactionResponse(
    int Id,
    int MemberId,
    string MemberName,
    string MemberColor,
    int CategoryId,
    string CategoryName,
    string CategoryIcon,
    string CategoryColor,
    string Type,
    decimal Amount,
    string? Note,
    DateTime TransactionDate,
    DateTime CreatedAt);
