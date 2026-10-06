using SummaryYearSalary.Domain.Common;
using SummaryYearSalary.Domain.Enums;

namespace SummaryYearSalary.Domain.Entities;

public class Transaction : BaseEntity
{
    public required string UserId { get; set; }
    public required string CreatedByUserId { get; set; }
    public int CategoryId { get; set; }
    public int? MemberId { get; set; }
    public TransactionType Type { get; set; }
    public decimal Amount { get; set; }
    public string? Note { get; set; }
    public DateTime TransactionDate { get; set; }

    public virtual Category Category { get; set; } = null!;
    public virtual Member? Member { get; set; }
}
