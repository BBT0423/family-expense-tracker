using SummaryYearSalary.Domain.Entities;

namespace SummaryYearSalary.Application.Interfaces.IRepositories;

public interface ITransactionProposalRepository : IRepository<TransactionProposal>
{
    Task<IEnumerable<TransactionProposal>> GetPendingByOwnerAsync(string ownerUserId);
}
