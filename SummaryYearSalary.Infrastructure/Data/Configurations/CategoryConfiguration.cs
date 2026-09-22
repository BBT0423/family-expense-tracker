using SummaryYearSalary.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SummaryYearSalary.Infrastructure.Data.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Name).IsRequired().HasMaxLength(100);
        builder.Property(e => e.Icon).IsRequired().HasMaxLength(20);
        builder.Property(e => e.Color).IsRequired().HasMaxLength(7);
        builder.Property(e => e.UserId).HasMaxLength(450);
        builder.HasIndex(e => new { e.UserId, e.Type });
    }
}
