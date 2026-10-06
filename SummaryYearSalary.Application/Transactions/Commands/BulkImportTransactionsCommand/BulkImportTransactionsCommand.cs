using SummaryYearSalary.Application.Common;
using SummaryYearSalary.Domain.Enums;

using MediatR;

namespace SummaryYearSalary.Application.Transactions.Commands.BulkImportTransactionsCommand;

public record BulkImportItem(
    int CategoryId,
    int? MemberId,
    TransactionType Type,
    decimal Amount,
    string? Note,
    DateTime TransactionDate);

public record BulkImportTransactionsCommand(
    string UserId,
    IEnumerable<BulkImportItem> Items) : IRequest<Result<int>>;
