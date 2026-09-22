using SummaryYearSalary.Domain.Common;
using SummaryYearSalary.Domain.Enums;

namespace SummaryYearSalary.Domain.Entities;

/// <summary>A transaction someone else logged for <see cref="OwnerUserId"/>; it only becomes a real Transaction once the owner accepts it.</summary>
public class TransactionProposal : BaseEntity
{
    public required string OwnerUserId { get; set; }
    public required string ProposedByUserId { get; set; }
    public TransactionType Type { get; set; }
    public decimal Amount { get; set; }
    public int CategoryId { get; set; }
    public string? Note { get; set; }
    public DateTime TransactionDate { get; set; }
    public TransactionProposalStatus Status { get; set; }
    public DateTime? DecidedAt { get; set; }

    public virtual Category Category { get; set; } = null!;
}
