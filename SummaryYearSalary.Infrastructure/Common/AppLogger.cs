using SummaryYearSalary.Application.Interfaces;
using SummaryYearSalary.Domain.Common;

using Serilog;

namespace SummaryYearSalary.Infrastructure.Common;

public class AppLogger(ILogger logger) : IAppLogger
{
    private readonly ILogger _logger = logger;

    public void LogDebug(string messageTemplate, params object?[]? propertyValues)
    {
        _logger.Information(messageTemplate, propertyValues);
    }

    public void LogDebug(string message)
    {
        _logger.Information(message);
    }

    public void LogDebug(Exception? exception, string messageTemplate, params object?[]? propertyValues)
    {
        _logger.Information(exception, messageTemplate, propertyValues);
    }

    public void CleanOldLogs(string logDirectory, int daysThreshold)
    {
        if (!Directory.Exists(logDirectory))
        {
            _logger.Information("The directory {LogDirectory} does not exist.", logDirectory);
            throw new DirectoryNotFoundException($"The directory {logDirectory} does not exist.");
        }

        var logFiles = Directory.GetFiles(logDirectory, "log-*.json");

        var thresholdDate = DateTimeProvider.GetBangkokTimeZone().AddDays(-daysThreshold);

        foreach (var file in logFiles)
        {
            var fileName = Path.GetFileNameWithoutExtension(file);
            if (DateTime.TryParseExact(fileName[4..], "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var logDate)
                && logDate < thresholdDate)
            {
                File.Delete(file);
                _logger.Information("Deleted log file: {File}", file);
            }
        }
    }
}