namespace SummaryYearSalary.Application.TransactionProposals.Responses;

public record TransactionProposalResponse(
    int Id,
    string ProposedByName,
    string Type,
    decimal Amount,
    int CategoryId,
    string CategoryName,
    string CategoryIcon,
    string CategoryColor,
    string? Note,
    DateTime TransactionDate,
    DateTime CreatedAt);
