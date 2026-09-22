using SummaryYearSalary.Application.Common;

using MediatR;

namespace SummaryYearSalary.Application.TransactionProposals.Commands.AcceptTransactionProposalCommand;

public record AcceptTransactionProposalCommand(string UserId, int ProposalId, int? CategoryId) : IRequest<Result<bool>>;
