using Serilog.Core;
using Serilog.Events;

namespace SummaryYearSalary.Api.Helpers;

internal sealed class LocalTimeEnricher: ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        TimeZoneInfo bangkokTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
        DateTimeOffset bangkokTime = TimeZoneInfo.ConvertTime(logEvent.Timestamp, bangkokTimeZone);
        var localTimeProperty = new LogEventProperty(
            "LocalTimestamp",
            new ScalarValue(bangkokTime.ToString("yyyy-MM-dd HH:mm:ss.fff zzz", System.Globalization.CultureInfo.InvariantCulture))
        );
        logEvent.AddPropertyIfAbsent(localTimeProperty);
    }
}