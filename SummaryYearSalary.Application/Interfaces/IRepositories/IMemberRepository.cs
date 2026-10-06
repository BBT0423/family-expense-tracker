using SummaryYearSalary.Domain.Entities;

namespace SummaryYearSalary.Application.Interfaces.IRepositories;

public interface IMemberRepository : IRepository<Member>
{
    Task<IEnumerable<Member>> GetAllActiveFirstAsync();
    Task<bool> HasTransactionsAsync(int memberId);
}
