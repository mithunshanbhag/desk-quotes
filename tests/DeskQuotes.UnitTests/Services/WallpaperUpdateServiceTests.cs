using WindowsWallpaperService = DeskQuotes.Services.Implementations.WindowsWallpaperService;

namespace DeskQuotes.UnitTests.Services;

public class WallpaperUpdateServiceTests
{
    #region Positive cases

    [Fact]
    public void TryUpdateWallpaper_WhenAllStepsSucceed_ReturnsTrue()
    {
        var quoteSelectionService = new SpyQuoteSelectionService();
        var monitorResolutionService = new SpyMonitorResolutionService();
        var wallpaperBackgroundColorService = new SpyWallpaperBackgroundColorService();
        var wallpaperRenderService = new SpyWallpaperRenderService();
        var windowsWallpaperService = new SpyWindowsWallpaperService { ReturnValue = true };
        var sut = new WallpaperUpdateService(
            quoteSelectionService,
            monitorResolutionService,
            wallpaperBackgroundColorService,
            wallpaperRenderService,
            windowsWallpaperService);
        var quote = new Quote { Text = "Build things that matter.", Author = "Unknown" };

        var result = sut.TryUpdateWallpaper([quote]);

        result.Should().BeTrue();
        quoteSelectionService.CallCount.Should().Be(1);
        monitorResolutionService.CallCount.Should().Be(1);
        wallpaperRenderService.CallCount.Should().Be(1);
        windowsWallpaperService.CallCount.Should().Be(1);
        wallpaperBackgroundColorService.GetNextAutomaticBackgroundColorCallCount.Should().Be(1);
        wallpaperBackgroundColorService.SetCurrentBackgroundColorCallCount.Should().Be(1);
        wallpaperRenderService.CapturedQuote.Should().BeSameAs(quote);
        wallpaperRenderService.CapturedBackgroundColor.ToArgb().Should().Be(wallpaperBackgroundColorService.NextAutomaticBackgroundColor.ToArgb());
        wallpaperBackgroundColorService.CapturedSetCurrentBackgroundColor?.ToArgb().Should().Be(wallpaperBackgroundColorService.NextAutomaticBackgroundColor.ToArgb());
        windowsWallpaperService.CapturedPath.Should().Be(wallpaperRenderService.RenderedPath);
    }

    [Fact]
    public void TryUpdateWallpaper_WhenExplicitBackgroundColorProvided_UsesExplicitColorInsteadOfAutomaticColor()
    {
        var quoteSelectionService = new SpyQuoteSelectionService();
        var monitorResolutionService = new SpyMonitorResolutionService();
        var wallpaperBackgroundColorService = new SpyWallpaperBackgroundColorService();
        var wallpaperRenderService = new SpyWallpaperRenderService();
        var windowsWallpaperService = new SpyWindowsWallpaperService { ReturnValue = true };
        var sut = new WallpaperUpdateService(
            quoteSelectionService,
            monitorResolutionService,
            wallpaperBackgroundColorService,
            wallpaperRenderService,
            windowsWallpaperService);
        var explicitBackgroundColor = Color.FromArgb(41, 55, 73);

        var result = sut.TryUpdateWallpaper([new Quote { Text = "Quote", Author = "Author" }], explicitBackgroundColor);

        result.Should().BeTrue();
        quoteSelectionService.CallCount.Should().Be(1);
        wallpaperBackgroundColorService.GetNextAutomaticBackgroundColorCallCount.Should().Be(0);
        wallpaperRenderService.CapturedBackgroundColor.ToArgb().Should().Be(explicitBackgroundColor.ToArgb());
        wallpaperBackgroundColorService.CapturedSetCurrentBackgroundColor?.ToArgb().Should().Be(explicitBackgroundColor.ToArgb());
    }

    [Fact]
    public void TryUpdateWallpaper_WhenExplicitBackgroundColorProvidedAfterSuccessfulRefresh_ReusesCurrentQuote()
    {
        var initiallySelectedQuote = new Quote { Text = "First quote", Author = "Author One" };
        var laterCandidateQuote = new Quote { Text = "Second quote", Author = "Author Two" };
        var quoteSelectionService = new SequenceQuoteSelectionService(initiallySelectedQuote, laterCandidateQuote);
        var monitorResolutionService = new SpyMonitorResolutionService();
        var wallpaperBackgroundColorService = new SpyWallpaperBackgroundColorService();
        var wallpaperRenderService = new SpyWallpaperRenderService();
        var windowsWallpaperService = new SpyWindowsWallpaperService { ReturnValue = true };
        var sut = new WallpaperUpdateService(
            quoteSelectionService,
            monitorResolutionService,
            wallpaperBackgroundColorService,
            wallpaperRenderService,
            windowsWallpaperService);
        var explicitBackgroundColor = Color.FromArgb(35, 48, 61);

        var initialRefreshResult = sut.TryUpdateWallpaper([initiallySelectedQuote, laterCandidateQuote]);
        var colorOnlyRefreshResult = sut.TryUpdateWallpaper([initiallySelectedQuote, laterCandidateQuote], explicitBackgroundColor);

        initialRefreshResult.Should().BeTrue();
        colorOnlyRefreshResult.Should().BeTrue();
        quoteSelectionService.CallCount.Should().Be(1);
        wallpaperRenderService.CapturedQuotes.Should().HaveCount(2);
        wallpaperRenderService.CapturedQuotes[0].Should().BeSameAs(initiallySelectedQuote);
        wallpaperRenderService.CapturedQuotes[1].Should().BeSameAs(initiallySelectedQuote);
        wallpaperRenderService.CapturedBackgroundColors[1].ToArgb().Should().Be(explicitBackgroundColor.ToArgb());
    }

    #endregion

    private sealed class SpyQuoteSelectionService : QuoteSelectionService
    {
        public int CallCount { get; private set; }
        public bool? ReturnValue { get; init; }
        public Quote? SelectedQuote { get; init; }

        public override bool TrySelectQuote(IEnumerable<Quote>? configuredQuotes, out Quote? selectedQuote)
        {
            CallCount++;

            if (!ReturnValue.HasValue) return base.TrySelectQuote(configuredQuotes, out selectedQuote);

            selectedQuote = SelectedQuote;
            return ReturnValue.Value;
        }
    }

    private sealed class SequenceQuoteSelectionService(params Quote[] selectedQuotes) : QuoteSelectionService
    {
        private readonly Queue<Quote> _selectedQuotes = new(selectedQuotes);

        public int CallCount { get; private set; }

        public override bool TrySelectQuote(IEnumerable<Quote>? configuredQuotes, out Quote? selectedQuote)
        {
            CallCount++;
            selectedQuote = _selectedQuotes.Count > 0
                ? _selectedQuotes.Dequeue()
                : null;
            return selectedQuote is not null;
        }
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
        public List<Color> CapturedBackgroundColors { get; } = [];
        public bool ThrowOnRender { get; init; }
        public Color CapturedBackgroundColor { get; private set; }
        public Quote? CapturedQuote { get; private set; }
        public List<Quote> CapturedQuotes { get; } = [];
        public string RenderedPath { get; } = Path.Combine(Path.GetTempPath(), "test-wallpaper.bmp");

        public override string RenderQuoteWallpaper(Quote quote, Size resolution, Color backgroundColor)
        {
            CallCount++;
            CapturedQuote = quote;
            CapturedBackgroundColor = backgroundColor;
            CapturedQuotes.Add(quote);
            CapturedBackgroundColors.Add(backgroundColor);

            if (ThrowOnRender) throw new InvalidOperationException("Render failed");

            return RenderedPath;
        }
    }

    private sealed class SpyWallpaperBackgroundColorService : WallpaperBackgroundColorService
    {
        public Color? CapturedSetCurrentBackgroundColor { get; private set; }
        public int GetNextAutomaticBackgroundColorCallCount { get; private set; }
        public Color NextAutomaticBackgroundColor { get; init; } = Color.FromArgb(24, 27, 36);
        public int SetCurrentBackgroundColorCallCount { get; private set; }

        public override Color GetNextAutomaticBackgroundColor()
        {
            GetNextAutomaticBackgroundColorCallCount++;
            return NextAutomaticBackgroundColor;
        }

        public override void SetCurrentBackgroundColor(Color backgroundColor)
        {
            SetCurrentBackgroundColorCallCount++;
            CapturedSetCurrentBackgroundColor = backgroundColor;
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

    #region Negative cases

    [Fact]
    public void TryUpdateWallpaper_WhenNoQuoteCanBeSelected_ReturnsFalseAndSkipsDependencies()
    {
        var quoteSelectionService = new SpyQuoteSelectionService { ReturnValue = false };
        var monitorResolutionService = new SpyMonitorResolutionService();
        var wallpaperBackgroundColorService = new SpyWallpaperBackgroundColorService();
        var wallpaperRenderService = new SpyWallpaperRenderService();
        var windowsWallpaperService = new SpyWindowsWallpaperService();
        var sut = new WallpaperUpdateService(
            quoteSelectionService,
            monitorResolutionService,
            wallpaperBackgroundColorService,
            wallpaperRenderService,
            windowsWallpaperService);

        var result = sut.TryUpdateWallpaper([]);

        result.Should().BeFalse();
        quoteSelectionService.CallCount.Should().Be(1);
        monitorResolutionService.CallCount.Should().Be(0);
        wallpaperRenderService.CallCount.Should().Be(0);
        windowsWallpaperService.CallCount.Should().Be(0);
        wallpaperBackgroundColorService.GetNextAutomaticBackgroundColorCallCount.Should().Be(0);
        wallpaperBackgroundColorService.SetCurrentBackgroundColorCallCount.Should().Be(0);
    }

    [Fact]
    public void TryUpdateWallpaper_WhenWallpaperRenderingThrows_ReturnsFalse()
    {
        var quoteSelectionService = new SpyQuoteSelectionService();
        var monitorResolutionService = new SpyMonitorResolutionService();
        var wallpaperBackgroundColorService = new SpyWallpaperBackgroundColorService();
        var wallpaperRenderService = new SpyWallpaperRenderService { ThrowOnRender = true };
        var windowsWallpaperService = new SpyWindowsWallpaperService();
        var sut = new WallpaperUpdateService(
            quoteSelectionService,
            monitorResolutionService,
            wallpaperBackgroundColorService,
            wallpaperRenderService,
            windowsWallpaperService);

        var result = sut.TryUpdateWallpaper([new Quote { Text = "Quote", Author = "Author" }]);

        result.Should().BeFalse();
        quoteSelectionService.CallCount.Should().Be(1);
        monitorResolutionService.CallCount.Should().Be(1);
        wallpaperRenderService.CallCount.Should().Be(1);
        windowsWallpaperService.CallCount.Should().Be(0);
        wallpaperBackgroundColorService.GetNextAutomaticBackgroundColorCallCount.Should().Be(1);
        wallpaperBackgroundColorService.SetCurrentBackgroundColorCallCount.Should().Be(0);
    }

    [Fact]
    public void TryUpdateWallpaper_WhenWallpaperApplyFails_ReturnsFalse()
    {
        var quoteSelectionService = new SpyQuoteSelectionService();
        var monitorResolutionService = new SpyMonitorResolutionService();
        var wallpaperBackgroundColorService = new SpyWallpaperBackgroundColorService();
        var wallpaperRenderService = new SpyWallpaperRenderService();
        var windowsWallpaperService = new SpyWindowsWallpaperService { ReturnValue = false };
        var sut = new WallpaperUpdateService(
            quoteSelectionService,
            monitorResolutionService,
            wallpaperBackgroundColorService,
            wallpaperRenderService,
            windowsWallpaperService);

        var result = sut.TryUpdateWallpaper([new Quote { Text = "Quote", Author = "Author" }]);

        result.Should().BeFalse();
        quoteSelectionService.CallCount.Should().Be(1);
        monitorResolutionService.CallCount.Should().Be(1);
        wallpaperRenderService.CallCount.Should().Be(1);
        windowsWallpaperService.CallCount.Should().Be(1);
        wallpaperBackgroundColorService.GetNextAutomaticBackgroundColorCallCount.Should().Be(1);
        wallpaperBackgroundColorService.SetCurrentBackgroundColorCallCount.Should().Be(0);
    }

    #endregion
}