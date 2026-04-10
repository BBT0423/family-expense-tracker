using SummaryYearSalary.Domain.Common;
using SummaryYearSalary.Domain.Entities;
using Microsoft.AspNetCore.Identity;

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace SummaryYearSalary.Infrastructure.Data;

public class SummaryYearSalaryDbContext : IdentityDbContext<ApplicationUser, IdentityRole, string>
{
    public SummaryYearSalaryDbContext(DbContextOptions<SummaryYearSalaryDbContext> options) : base(options)
    {
    }

    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<FamilyMember> FamilyMembers => Set<FamilyMember>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<Budget> Budgets => Set<Budget>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configure ASP.NET Identity table names
        builder.Entity<ApplicationUser>().ToTable("IdentityUsers");
        builder.Entity<IdentityRole>().ToTable("IdentityRoles");
        builder.Entity<IdentityUserRole<string>>().ToTable("IdentityUserRoles");
        builder.Entity<IdentityUserClaim<string>>().ToTable("IdentityUserClaims");
        builder.Entity<IdentityUserLogin<string>>().ToTable("IdentityUserLogins");
        builder.Entity<IdentityUserToken<string>>().ToTable("IdentityUserTokens");
        builder.Entity<IdentityRoleClaim<string>>().ToTable("IdentityRoleClaims");

        // Configure ApplicationUser
        builder.Entity<ApplicationUser>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
            
            // Relationships
            entity.HasMany(e => e.RefreshTokens)
                .WithOne(e => e.User)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
                
            // Configure RefreshToken
            builder.Entity<RefreshToken>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Token).IsUnique();
                entity.Property(e => e.Token).IsRequired().HasMaxLength(500);
            });

            // Configure AuditLog
            builder.Entity<AuditLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Timestamp);
                entity.HasIndex(e => e.UserId);
                entity.Property(e => e.Action).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Resource).IsRequired().HasMaxLength(200);
            });
        });


        // Apply configurations
        builder.ApplyConfigurationsFromAssembly(typeof(SummaryYearSalaryDbContext).Assembly);

        // Global query filter for soft delete
        foreach (var clrType in builder.Model.GetEntityTypes()
            .Where(et => typeof(BaseEntity).IsAssignableFrom(et.ClrType))
            .Select(et => et.ClrType))
        {
            builder.Entity(clrType)
                .HasQueryFilter(CreateIsDeletedFilter(clrType));
        }
    }

    private static LambdaExpression CreateIsDeletedFilter(Type entityType)
    {
        var parameter = Expression.Parameter(entityType, "e");
        var property = Expression.Property(parameter, nameof(BaseEntity.IsDeleted));
        var condition = Expression.Equal(property, Expression.Constant(false));
        return Expression.Lambda(condition, parameter);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTimeProvider.GetBangkokTimeZone();
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTimeProvider.GetBangkokTimeZone();
                    break;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
