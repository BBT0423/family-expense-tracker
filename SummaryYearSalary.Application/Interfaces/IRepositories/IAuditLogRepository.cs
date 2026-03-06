using SummaryYearSalary.Application.Common;
using SummaryYearSalary.Domain.Entities;

namespace SummaryYearSalary.Application.Interfaces.IRepositories;

public interface IAuditLogRepository : IRepository<AuditLog>
{
    Task<IEnumerable<AuditLog>> GetByUserAsync(string userId, int count = 100);
    Task<IEnumerable<AuditLog>> GetRecentAsync(int count = 100);
    Task LogActionAsync(AuditLogRequest request);
}