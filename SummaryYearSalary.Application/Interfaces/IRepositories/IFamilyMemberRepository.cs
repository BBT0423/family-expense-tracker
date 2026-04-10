using SummaryYearSalary.Domain.Entities;

namespace SummaryYearSalary.Application.Interfaces.IRepositories;

public interface IFamilyMemberRepository : IRepository<FamilyMember>
{
    Task<IEnumerable<FamilyMember>> GetByUserIdAsync(string userId);
    Task<bool> ExistsByUserIdAndNameAsync(string userId, string name, int? excludeId = null);
}
