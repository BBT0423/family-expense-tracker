namespace SummaryYearSalary.Domain.Common;

public static class DateTimeProvider
{
    private static readonly TimeZoneInfo BangkokTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
    public static DateTime GetBangkokTimeZone()
    {
        DateTime bangkokTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, BangkokTimeZone);
        return bangkokTime;
    }

    public static DateTime GetBangkokTimeZone(this DateTime dateTime)
    {
        DateTime bangkokTime = TimeZoneInfo.ConvertTimeFromUtc(dateTime, BangkokTimeZone);
        return bangkokTime;
    }
}
