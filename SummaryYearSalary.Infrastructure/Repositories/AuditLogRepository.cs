using SummaryYearSalary.Application.Common;
using SummaryYearSalary.Application.Interfaces.IRepositories;
using SummaryYearSalary.Domain.Common;
using SummaryYearSalary.Domain.Entities;
using SummaryYearSalary.Infrastructure.Data;

using Microsoft.EntityFrameworkCore;

namespace SummaryYearSalary.Infrastructure.Repositories;

public class AuditLogRepository(SummaryYearSalaryDbContext context) : Repository<AuditLog>(context), IAuditLogRepository
{
    public async Task<IEnumerable<AuditLog>> GetByUserAsync(string userId, int count = 100)
    {
        return await _context.AuditLogs
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.Timestamp)
            .Take(count)
            .ToListAsync();
    }

    public async Task<IEnumerable<AuditLog>> GetRecentAsync(int count = 100)
    {
        return await _context.AuditLogs
            .OrderByDescending(a => a.Timestamp)
            .Take(count)
            .ToListAsync();
    }

    public async Task LogActionAsync(AuditLogRequest request)
    {
        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid().ToString(),
            UserId = request.UserId,
            UserName = request.UserName,
            Action = request.Action,
            Resource = request.Resource,
            IpAddress = request.IpAddress,
            UserAgent = request.UserAgent,
            Success = request.Success,
            ErrorMessage = request.ErrorMessage,
            Timestamp = DateTimeProvider.GetBangkokTimeZone()
        };

        await AddAsync(auditLog);
        await _context.SaveChangesAsync();
    }
}