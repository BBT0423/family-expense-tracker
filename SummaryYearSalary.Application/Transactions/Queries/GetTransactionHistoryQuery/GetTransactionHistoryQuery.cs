using SummaryYearSalary.Application.Common;
using SummaryYearSalary.Application.Transactions.Responses;

using MediatR;

namespace SummaryYearSalary.Application.Transactions.Queries.GetTransactionHistoryQuery;

public record GetTransactionHistoryQuery(
    int TransactionId,
    string UserId) : IRequest<Result<List<TransactionHistoryResponse>>>;
