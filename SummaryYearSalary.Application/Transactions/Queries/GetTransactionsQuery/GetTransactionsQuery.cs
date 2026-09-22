using SummaryYearSalary.Application.Common;
using SummaryYearSalary.Application.Transactions.Responses;
using SummaryYearSalary.Domain.Enums;

using MediatR;

namespace SummaryYearSalary.Application.Transactions.Queries.GetTransactionsQuery;

public record GetTransactionsQuery(
    string UserId,
    int Month,
    int Year,
    TransactionType? Type,
    int? CategoryId,
    string? SearchTerm,
    int PageNumber,
    int PageSize,
    string? ScopeUserId = null) : IRequest<Result<PaginatedList<TransactionResponse>>>;
