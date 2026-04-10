namespace SummaryYearSalary.Application.Reports.Responses;

public record CategoryBreakdown(
    int CategoryId,
    string Name,
    string Icon,
    string Color,
    decimal Total,
    double Percentage,
    int Count);

public record CategorySummaryResponse(IEnumerable<CategoryBreakdown> Categories);
