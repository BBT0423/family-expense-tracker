namespace SummaryYearSalary.Application.Reports.Responses;

public record MonthlySummaryResponse(
    decimal TotalIncome,
    decimal TotalExpense,
    decimal Balance,
    int TransactionCount);
