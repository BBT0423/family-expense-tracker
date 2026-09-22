namespace SummaryYearSalary.Application.Notifications.Responses;

public record NotificationResponse(
    int Id,
    string Type,
    string Title,
    string Message,
    int? RelatedEntityId,
    bool IsRead,
    DateTime CreatedAt);
