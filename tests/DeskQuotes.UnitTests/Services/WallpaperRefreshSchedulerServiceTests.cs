namespace DeskQuotes.UnitTests.Services;

public class WallpaperRefreshSchedulerServiceTests
{
    private readonly WallpaperRefreshSchedulerService _sut = new();

    #region Boundary cases

    [Fact]
    public void GetDelayUntilNextLocalTopOfHourRefresh_WhenExactlyAtTopOfHour_ReturnsOneHour()
    {
        var localNow = new DateTimeOffset(2026, 1, 1, 10, 0, 0, TimeSpan.FromHours(5.5));

        var delay = WallpaperRefreshSchedulerService.GetDelayUntilNextLocalTopOfHourRefresh(localNow);

        delay.Should().Be(TimeSpan.FromHours(1));
    }

    [Fact]
    public void GetDelayUntilNextLocalTopOfHourRefresh_WhenNearTopOfHour_ReturnsRemainingTime()
    {
        var localNow = new DateTimeOffset(2026, 1, 1, 10, 59, 59, 500, TimeSpan.Zero);

        var delay = WallpaperRefreshSchedulerService.GetDelayUntilNextLocalTopOfHourRefresh(localNow);

        delay.Should().Be(TimeSpan.FromMilliseconds(500));
    }

    [Fact]
    public void GetDelayUntilNextLocalTopOfHourRefresh_WhenCrossingMidnight_ReturnsRemainingTime()
    {
        var localNow = new DateTimeOffset(2026, 1, 1, 23, 59, 59, 900, TimeSpan.Zero);

        var delay = WallpaperRefreshSchedulerService.GetDelayUntilNextLocalTopOfHourRefresh(localNow);

        delay.Should().Be(TimeSpan.FromMilliseconds(100));
    }

    #endregion
}
