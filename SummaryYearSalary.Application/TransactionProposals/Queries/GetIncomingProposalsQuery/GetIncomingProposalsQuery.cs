using SummaryYearSalary.Application.Common;
using SummaryYearSalary.Application.TransactionProposals.Responses;

using MediatR;

namespace SummaryYearSalary.Application.TransactionProposals.Queries.GetIncomingProposalsQuery;

public record GetIncomingProposalsQuery(string UserId) : IRequest<Result<IEnumerable<TransactionProposalResponse>>>;
