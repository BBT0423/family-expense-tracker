using SummaryYearSalary.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SummaryYearSalary.Infrastructure.Data.Configurations;

public class BudgetConfiguration : IEntityTypeConfiguration<Budget>
{
    public void Configure(EntityTypeBuilder<Budget> builder)
    {
        builder.ToTable("Budgets");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.LimitAmount).HasPrecision(18, 2);
        builder.Property(e => e.UserId).IsRequired().HasMaxLength(450);
        builder.HasIndex(e => new { e.UserId, e.CategoryId, e.Month, e.Year }).IsUnique();

        builder.HasOne(e => e.Category)
            .WithMany(e => e.Budgets)
            .HasForeignKey(e => e.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
