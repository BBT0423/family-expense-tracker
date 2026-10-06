using SummaryYearSalary.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SummaryYearSalary.Infrastructure.Data.Configurations;

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("Transactions");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Amount).HasPrecision(18, 2);
        builder.Property(e => e.Note).HasMaxLength(500);
        builder.Property(e => e.UserId).IsRequired().HasMaxLength(450);
        builder.Property(e => e.CreatedByUserId).IsRequired().HasMaxLength(450);
        builder.HasIndex(e => new { e.UserId, e.TransactionDate });

        builder.HasOne(e => e.Category)
            .WithMany(e => e.Transactions)
            .HasForeignKey(e => e.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Member)
            .WithMany(e => e.Transactions)
            .HasForeignKey(e => e.MemberId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
