using SummaryYearSalary.Domain.Common;
using SummaryYearSalary.Domain.Enums;

namespace SummaryYearSalary.Domain.Entities;

public class Category : BaseEntity
{
    public string? UserId { get; set; } // null = system default (seeded for all users)
    public string Name { get; set; } = string.Empty;
    public TransactionType Type { get; set; }
    public string Icon { get; set; } = "💰";
    public string Color { get; set; } = "#4A90D9";
    public bool IsDefault { get; set; }

    public virtual ICollection<Transaction> Transactions { get; set; } = [];
    public virtual ICollection<Budget> Budgets { get; set; } = [];
}
