using SummaryYearSalary.Domain.Common;

namespace SummaryYearSalary.Domain.Entities;

public class FamilyMember : BaseEntity
{
    public required string UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = "#4A90D9";
    public string? Avatar { get; set; }
    public bool IsActive { get; set; } = true;

    public virtual ICollection<Transaction> Transactions { get; set; } = [];
}
