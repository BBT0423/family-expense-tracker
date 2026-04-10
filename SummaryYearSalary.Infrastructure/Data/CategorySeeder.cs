using SummaryYearSalary.Domain.Common;
using SummaryYearSalary.Domain.Entities;
using SummaryYearSalary.Domain.Enums;

using Microsoft.EntityFrameworkCore;

namespace SummaryYearSalary.Infrastructure.Data;

public static class CategorySeeder
{
    public static async Task SeedDefaultCategoriesAsync(SummaryYearSalaryDbContext context)
    {
        if (await context.Categories.AnyAsync(c => c.IsDefault))
            return;

        var now = DateTimeProvider.GetBangkokTimeZone();
        var categories = new List<Category>
        {
            // Income categories
            new() { Name = "เงินเดือน", Type = TransactionType.Income, Icon = "💼", Color = "#27ae60", IsDefault = true, CreatedAt = now },
            new() { Name = "โบนัส", Type = TransactionType.Income, Icon = "🎁", Color = "#2ecc71", IsDefault = true, CreatedAt = now },
            new() { Name = "รายได้เสริม", Type = TransactionType.Income, Icon = "💡", Color = "#1abc9c", IsDefault = true, CreatedAt = now },
            new() { Name = "ดอกเบี้ย/ปันผล", Type = TransactionType.Income, Icon = "🏦", Color = "#3498db", IsDefault = true, CreatedAt = now },
            new() { Name = "อื่นๆ (รายรับ)", Type = TransactionType.Income, Icon = "➕", Color = "#9b59b6", IsDefault = true, CreatedAt = now },
            // Expense categories
            new() { Name = "อาหาร/เครื่องดื่ม", Type = TransactionType.Expense, Icon = "🍜", Color = "#e67e22", IsDefault = true, CreatedAt = now },
            new() { Name = "ที่พักอาศัย", Type = TransactionType.Expense, Icon = "🏠", Color = "#e74c3c", IsDefault = true, CreatedAt = now },
            new() { Name = "การเดินทาง", Type = TransactionType.Expense, Icon = "🚗", Color = "#c0392b", IsDefault = true, CreatedAt = now },
            new() { Name = "ช้อปปิ้ง", Type = TransactionType.Expense, Icon = "🛍️", Color = "#8e44ad", IsDefault = true, CreatedAt = now },
            new() { Name = "สุขภาพ", Type = TransactionType.Expense, Icon = "💊", Color = "#e74c3c", IsDefault = true, CreatedAt = now },
            new() { Name = "บันเทิง", Type = TransactionType.Expense, Icon = "🎬", Color = "#f39c12", IsDefault = true, CreatedAt = now },
            new() { Name = "การศึกษา", Type = TransactionType.Expense, Icon = "📚", Color = "#2980b9", IsDefault = true, CreatedAt = now },
            new() { Name = "ประกัน", Type = TransactionType.Expense, Icon = "🛡️", Color = "#16a085", IsDefault = true, CreatedAt = now },
            new() { Name = "สาธารณูปโภค", Type = TransactionType.Expense, Icon = "⚡", Color = "#f1c40f", IsDefault = true, CreatedAt = now },
            new() { Name = "อื่นๆ (รายจ่าย)", Type = TransactionType.Expense, Icon = "➖", Color = "#95a5a6", IsDefault = true, CreatedAt = now },
        };

        await context.Categories.AddRangeAsync(categories);
        await context.SaveChangesAsync();
    }
}
