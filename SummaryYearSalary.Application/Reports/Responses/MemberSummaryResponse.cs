namespace SummaryYearSalary.Application.Reports.Responses;

public record MemberBreakdown(
    int MemberId,
    string Name,
    string Color,
    decimal TotalExpense,
    decimal TotalIncome,
    double Percentage);

public record MemberSummaryResponse(IEnumerable<MemberBreakdown> Members);
