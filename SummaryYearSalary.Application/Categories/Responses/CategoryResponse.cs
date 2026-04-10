namespace SummaryYearSalary.Application.Categories.Responses;

public record CategoryResponse(
    int Id,
    string Name,
    string Type,
    string Icon,
    string Color,
    bool IsDefault);
