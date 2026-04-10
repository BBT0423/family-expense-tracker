using SummaryYearSalary.Domain.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SummaryYearSalary.Infrastructure.Data.Configurations;

public class FamilyMemberConfiguration : IEntityTypeConfiguration<FamilyMember>
{
    public void Configure(EntityTypeBuilder<FamilyMember> builder)
    {
        builder.ToTable("FamilyMembers");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.UserId).IsRequired().HasMaxLength(450);
        builder.Property(e => e.Name).IsRequired().HasMaxLength(100);
        builder.Property(e => e.Color).IsRequired().HasMaxLength(7);
        builder.Property(e => e.Avatar).HasMaxLength(500);
        builder.HasIndex(e => e.UserId);
    }
}
