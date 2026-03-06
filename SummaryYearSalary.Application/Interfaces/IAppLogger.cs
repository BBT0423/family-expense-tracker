namespace SummaryYearSalary.Application.Interfaces;

public interface IAppLogger
{
    void LogDebug(string message);
    void LogDebug(string messageTemplate, params object?[]? propertyValues);
    void LogDebug(Exception? exception, string messageTemplate, params object?[]? propertyValues);
    void CleanOldLogs(string logDirectory, int daysThreshold);
}