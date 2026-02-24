namespace DeskQuotes.UnitTests.Services;

public class WallpaperUpdateServiceTests
{
    [Fact]
    public void TryUpdateWallpaper_WhenAllStepsSucceed_ReturnsTrue()
    {
        var quoteSelectionService = new QuoteSelectionService();
        var monitorResolutionService = new SpyMonitorResolutionService();
        var wallpaperRenderService = new SpyWallpaperRenderService();
        var windowsWallpaperService = new SpyWindowsWallpaperService { ReturnValue = true };
        var sut = new WallpaperUpdateService(
            quoteSelectionService,
            monitorResolutionService,
            wallpaperRenderService,
            windowsWallpaperService);
        var quote = new Quote { Text = "Build things that matter.", Author = "Unknown" };

        var result = sut.TryUpdateWallpaper([quote]);

        result.Should().BeTrue();
        monitorResolutionService.CallCount.Should().Be(1);
        wallpaperRenderService.CallCount.Should().Be(1);
        windowsWallpaperService.CallCount.Should().Be(1);
        wallpaperRenderService.CapturedQuote.Should().BeSameAs(quote);
        windowsWallpaperService.CapturedPath.Should().Be(wallpaperRenderService.RenderedPath);
    }

    [Fact]
    public void TryUpdateWallpaper_WhenNoQuoteCanBeSelected_ReturnsFalseAndSkipsDependencies()
    {
        var monitorResolutionService = new SpyMonitorResolutionService();
        var wallpaperRenderService = new SpyWallpaperRenderService();
        var windowsWallpaperService = new SpyWindowsWallpaperService();
        var sut = new WallpaperUpdateService(
            new QuoteSelectionService(),
            monitorResolutionService,
            wallpaperRenderService,
            windowsWallpaperService);

        var result = sut.TryUpdateWallpaper([]);

        result.Should().BeFalse();
        monitorResolutionService.CallCount.Should().Be(0);
        wallpaperRenderService.CallCount.Should().Be(0);
        windowsWallpaperService.CallCount.Should().Be(0);
    }

    [Fact]
    public void TryUpdateWallpaper_WhenWallpaperRenderingThrows_ReturnsFalse()
    {
        var monitorResolutionService = new SpyMonitorResolutionService();
        var wallpaperRenderService = new SpyWallpaperRenderService { ThrowOnRender = true };
        var windowsWallpaperService = new SpyWindowsWallpaperService();
        var sut = new WallpaperUpdateService(
            new QuoteSelectionService(),
            monitorResolutionService,
            wallpaperRenderService,
            windowsWallpaperService);

        var result = sut.TryUpdateWallpaper([new Quote { Text = "Quote", Author = "Author" }]);

        result.Should().BeFalse();
        monitorResolutionService.CallCount.Should().Be(1);
        wallpaperRenderService.CallCount.Should().Be(1);
        windowsWallpaperService.CallCount.Should().Be(0);
    }

    [Fact]
    public void TryUpdateWallpaper_WhenWallpaperApplyFails_ReturnsFalse()
    {
        var monitorResolutionService = new SpyMonitorResolutionService();
        var wallpaperRenderService = new SpyWallpaperRenderService();
        var windowsWallpaperService = new SpyWindowsWallpaperService { ReturnValue = false };
        var sut = new WallpaperUpdateService(
            new QuoteSelectionService(),
            monitorResolutionService,
            wallpaperRenderService,
            windowsWallpaperService);

        var result = sut.TryUpdateWallpaper([new Quote { Text = "Quote", Author = "Author" }]);

        result.Should().BeFalse();
        monitorResolutionService.CallCount.Should().Be(1);
        wallpaperRenderService.CallCount.Should().Be(1);
        windowsWallpaperService.CallCount.Should().Be(1);
    }

    private sealed class SpyMonitorResolutionService : MonitorResolutionService
    {
        public int CallCount { get; private set; }

        public override Size InferWallpaperResolution()
        {
            CallCount++;
            return new Size(1920, 1080);
        }
    }

    private sealed class SpyWallpaperRenderService : WallpaperRenderService
    {
        public int CallCount { get; private set; }
        public bool ThrowOnRender { get; init; }
        public Quote? CapturedQuote { get; private set; }
        public string RenderedPath { get; } = Path.Combine(Path.GetTempPath(), "test-wallpaper.bmp");

        public override string RenderQuoteWallpaper(Quote quote, Size resolution)
        {
            CallCount++;
            CapturedQuote = quote;

            if (ThrowOnRender) throw new InvalidOperationException("Render failed");

            return RenderedPath;
        }
    }

    private sealed class SpyWindowsWallpaperService : WindowsWallpaperService
    {
        public int CallCount { get; private set; }
        public bool ReturnValue { get; init; }
        public string? CapturedPath { get; private set; }

        public override bool TryApplyWallpaper(string wallpaperPath)
        {
            CallCount++;
            CapturedPath = wallpaperPath;
            return ReturnValue;
        }
    }
}