namespace SummaryYearSalary.Application.Reports.Responses;

public record MonthlyBreakdown(int Month, decimal Income, decimal Expense);

public record YearlySummaryResponse(IEnumerable<MonthlyBreakdown> Months);
