namespace WebApp.Infrastructure;

public static class DateTimeDisplay
{
    private static readonly TimeZoneInfo AppTimeZone = ResolveTimeZone();

    public static string Format(DateTime value, string format = "g")
    {
        return ToAppLocalTime(value).ToString(format);
    }

    public static DateTime ToAppLocalTime(DateTime value)
    {
        var utc = value.Kind == DateTimeKind.Utc
            ? value
            : DateTime.SpecifyKind(value, DateTimeKind.Utc);

        return TimeZoneInfo.ConvertTimeFromUtc(utc, AppTimeZone);
    }

    private static TimeZoneInfo ResolveTimeZone()
    {
        foreach (var id in new[] { "Russian Standard Time", "Europe/Moscow" })
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(id);
            }
            catch
            {
                // Try the next platform-specific identifier.
            }
        }

        return TimeZoneInfo.Local;
    }
}
