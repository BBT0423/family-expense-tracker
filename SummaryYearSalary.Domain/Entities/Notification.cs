using SummaryYearSalary.Domain.Common;
using SummaryYearSalary.Domain.Enums;

namespace SummaryYearSalary.Domain.Entities;

public class Notification : BaseEntity
{
    public required string UserId { get; set; }
    public NotificationType Type { get; set; }
    public required string Title { get; set; }
    public required string Message { get; set; }
    public int? RelatedEntityId { get; set; }
    public bool IsRead { get; set; }
}
