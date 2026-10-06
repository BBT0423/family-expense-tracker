using SummaryYearSalary.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SummaryYearSalary.Infrastructure.Data.Configurations;

public class TransactionHistoryConfiguration : IEntityTypeConfiguration<TransactionHistory>
{
    public void Configure(EntityTypeBuilder<TransactionHistory> builder)
    {
        builder.ToTable("TransactionHistories");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.FieldName).IsRequired().HasMaxLength(50);
        builder.Property(e => e.OldValue).HasMaxLength(500);
        builder.Property(e => e.NewValue).HasMaxLength(500);
        builder.Property(e => e.ChangedByUserId).IsRequired().HasMaxLength(450);
        builder.HasIndex(e => e.TransactionId);

        builder.HasOne(e => e.Transaction)
            .WithMany()
            .HasForeignKey(e => e.TransactionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
