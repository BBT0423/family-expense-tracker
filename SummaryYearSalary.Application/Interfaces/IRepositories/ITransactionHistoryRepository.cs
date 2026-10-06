using SummaryYearSalary.Domain.Entities;

namespace SummaryYearSalary.Application.Interfaces.IRepositories;

public interface ITransactionHistoryRepository : IRepository<TransactionHistory>
{
    Task<IEnumerable<TransactionHistory>> GetByTransactionIdAsync(int transactionId);
}
