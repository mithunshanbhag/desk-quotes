namespace DeskQuotes.Services.Implementations;

public sealed class WallpaperRefreshSchedulerService
{
    public static TimeSpan GetDelayUntilNextLocalTopOfHourRefresh(DateTimeOffset localNow)
    {
        var nextLocalTopOfHour = new DateTimeOffset(
            localNow.Year,
            localNow.Month,
            localNow.Day,
            localNow.Hour,
            0,
            0,
            localNow.Offset).AddHours(1);

        return nextLocalTopOfHour - localNow;
    }
}