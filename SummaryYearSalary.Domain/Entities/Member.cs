using SummaryYearSalary.Domain.Common;

namespace SummaryYearSalary.Domain.Entities;

public class Member : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = "#4A90D9";
    public bool IsActive { get; set; } = true;

    public virtual ICollection<Transaction> Transactions { get; set; } = [];
}
