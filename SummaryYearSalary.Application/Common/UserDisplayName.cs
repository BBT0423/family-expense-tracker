using SummaryYearSalary.Domain.Entities;

namespace SummaryYearSalary.Application.Common;

public static class UserDisplayName
{
    /// <summary>First + last name, falling back to the e-mail address, then to the given fallback text.</summary>
    public static string Of(ApplicationUser? user, string fallback = "ผู้ใช้")
    {
        if (user is null) return fallback;
        var name = string.Join(' ', new[] { user.FirstName, user.LastName }.Where(s => !string.IsNullOrWhiteSpace(s)));
        return string.IsNullOrWhiteSpace(name) ? user.Email ?? fallback : name;
    }
}
