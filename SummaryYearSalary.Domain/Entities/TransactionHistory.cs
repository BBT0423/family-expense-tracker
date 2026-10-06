using SummaryYearSalary.Domain.Common;

namespace SummaryYearSalary.Domain.Entities;

public class TransactionHistory : BaseEntity
{
    public required int TransactionId { get; set; }
    public required string ChangedByUserId { get; set; }
    public required string FieldName { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }

    public virtual Transaction Transaction { get; set; } = null!;
}
