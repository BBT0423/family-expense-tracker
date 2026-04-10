using SummaryYearSalary.Application.Common;

using MediatR;

namespace SummaryYearSalary.Application.Transactions.Commands.DeleteTransactionCommand;

public record DeleteTransactionCommand(int Id, string UserId) : IRequest<Result<bool>>;
