using SummaryYearSalary.Domain.Common;

namespace SummaryYearSalary.Domain.Entities;

public class Budget : BaseEntity
{
    public required string UserId { get; set; }
    public int CategoryId { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
    public decimal LimitAmount { get; set; }

    public virtual Category Category { get; set; } = null!;
}
