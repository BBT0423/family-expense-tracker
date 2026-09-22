using SummaryYearSalary.Domain.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SummaryYearSalary.Infrastructure.Data.Configurations;

public class TransactionProposalConfiguration : IEntityTypeConfiguration<TransactionProposal>
{
    public void Configure(EntityTypeBuilder<TransactionProposal> builder)
    {
        builder.ToTable("TransactionProposals");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.OwnerUserId).IsRequired().HasMaxLength(450);
        builder.Property(e => e.ProposedByUserId).IsRequired().HasMaxLength(450);
        builder.Property(e => e.Amount).HasPrecision(18, 2);
        builder.Property(e => e.Note).HasMaxLength(500);
        builder.HasIndex(e => new { e.OwnerUserId, e.Status });

        builder.HasOne(e => e.Category)
            .WithMany()
            .HasForeignKey(e => e.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
