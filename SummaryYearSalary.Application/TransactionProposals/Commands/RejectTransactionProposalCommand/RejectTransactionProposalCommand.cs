using SummaryYearSalary.Application.Common;

using MediatR;

namespace SummaryYearSalary.Application.TransactionProposals.Commands.RejectTransactionProposalCommand;

public record RejectTransactionProposalCommand(string UserId, int ProposalId) : IRequest<Result<bool>>;
