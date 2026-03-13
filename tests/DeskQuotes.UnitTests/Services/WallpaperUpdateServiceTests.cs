using WindowsWallpaperService = DeskQuotes.Services.Implementations.WindowsWallpaperService;

namespace DeskQuotes.UnitTests.Services;

public class WallpaperUpdateServiceTests
{
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
        public List<string> CapturedFontFamilyNames { get; } = [];
        public bool ThrowOnRender { get; init; }
        public Color CapturedBackgroundColor { get; private set; }
        public string? CapturedFontFamilyName { get; private set; }
        public Quote? CapturedQuote { get; private set; }
        public List<Quote> CapturedQuotes { get; } = [];
        public string RenderedPath { get; } = Path.Combine(Path.GetTempPath(), "test-wallpaper.bmp");

        public override string RenderQuoteWallpaper(Quote quote, Size resolution, Color backgroundColor, string fontFamilyName)
        {
            CallCount++;
            CapturedQuote = quote;
            CapturedBackgroundColor = backgroundColor;
            CapturedFontFamilyName = fontFamilyName;
            CapturedQuotes.Add(quote);
            CapturedBackgroundColors.Add(backgroundColor);
            CapturedFontFamilyNames.Add(fontFamilyName);

            if (ThrowOnRender) throw new InvalidOperationException("Render failed");

            return RenderedPath;
        }
    }

    private sealed class SpyWallpaperFontSelectionService : WallpaperFontSelectionService
    {
        public int CallCount { get; private set; }
        public int ExcludingCurrentFontCallCount { get; private set; }
        public string? ExcludedFontFamilyName { get; private set; }
        public string NextDifferentFontFamilyName { get; } = "Segoe UI";
        public string NextFontFamilyName { get; } = "Georgia";

        public override string GetRandomFontFamilyName()
        {
            CallCount++;
            return NextFontFamilyName;
        }

        public override string GetRandomFontFamilyName(string? excludedFontFamilyName)
        {
            ExcludingCurrentFontCallCount++;
            ExcludedFontFamilyName = excludedFontFamilyName;
            return NextDifferentFontFamilyName;
        }
    }

    private sealed class SequenceWallpaperFontSelectionService(params string[] fontFamilyNames) : WallpaperFontSelectionService
    {
        private readonly Queue<string> _fontFamilyNames = new(fontFamilyNames);

        public int CallCount { get; private set; }

        public override string GetRandomFontFamilyName()
        {
            CallCount++;
            return _fontFamilyNames.Dequeue();
        }
    }

    private sealed class SpyWallpaperBackgroundColorService : WallpaperBackgroundColorService
    {
        public Color? CapturedSetCurrentBackgroundColor { get; private set; }
        public Color CurrentBackgroundColor { get; private set; } = Color.FromArgb(24, 27, 36);
        public int GetCurrentBackgroundColorCallCount { get; private set; }
        public int GetNextAutomaticBackgroundColorCallCount { get; private set; }
        public Color NextAutomaticBackgroundColor { get; } = Color.FromArgb(24, 27, 36);
        public int SetCurrentBackgroundColorCallCount { get; private set; }

        public override Color GetCurrentBackgroundColor()
        {
            GetCurrentBackgroundColorCallCount++;
            return CurrentBackgroundColor;
        }

        public override Color GetNextAutomaticBackgroundColor()
        {
            GetNextAutomaticBackgroundColorCallCount++;
            return NextAutomaticBackgroundColor;
        }

        public override void SetCurrentBackgroundColor(Color backgroundColor)
        {
            SetCurrentBackgroundColorCallCount++;
            CapturedSetCurrentBackgroundColor = backgroundColor;
            CurrentBackgroundColor = backgroundColor;
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

    #region Positive cases

    [Fact]
    public void TryUpdateWallpaper_WhenAllStepsSucceed_ReturnsTrue()
    {
        var quoteSelectionService = new SpyQuoteSelectionService();
        var monitorResolutionService = new SpyMonitorResolutionService();
        var wallpaperBackgroundColorService = new SpyWallpaperBackgroundColorService();
        var wallpaperFontSelectionService = new SpyWallpaperFontSelectionService();
        var wallpaperRenderService = new SpyWallpaperRenderService();
        var windowsWallpaperService = new SpyWindowsWallpaperService { ReturnValue = true };
        var sut = new WallpaperUpdateService(
            quoteSelectionService,
            monitorResolutionService,
            wallpaperBackgroundColorService,
            wallpaperFontSelectionService,
            wallpaperRenderService,
            windowsWallpaperService);
        var quote = new Quote { Text = "Build things that matter.", Author = "Unknown" };

        var result = sut.TryUpdateWallpaper([quote]);

        Assert.True(result);
        Assert.Equal(1, quoteSelectionService.CallCount);
        Assert.Equal(1, monitorResolutionService.CallCount);
        Assert.Equal(1, wallpaperRenderService.CallCount);
        Assert.Equal(1, windowsWallpaperService.CallCount);
        Assert.Equal(1, wallpaperBackgroundColorService.GetNextAutomaticBackgroundColorCallCount);
        Assert.Equal(1, wallpaperBackgroundColorService.SetCurrentBackgroundColorCallCount);
        Assert.Equal(1, wallpaperFontSelectionService.CallCount);
        Assert.Same(quote, wallpaperRenderService.CapturedQuote);
        Assert.Equal(wallpaperBackgroundColorService.NextAutomaticBackgroundColor.ToArgb(), wallpaperRenderService.CapturedBackgroundColor.ToArgb());
        Assert.Equal(wallpaperFontSelectionService.NextFontFamilyName, wallpaperRenderService.CapturedFontFamilyName);
        Assert.Equal(wallpaperBackgroundColorService.NextAutomaticBackgroundColor.ToArgb(), wallpaperBackgroundColorService.CapturedSetCurrentBackgroundColor?.ToArgb());
        Assert.Equal(wallpaperRenderService.RenderedPath, windowsWallpaperService.CapturedPath);
    }

    [Fact]
    public void TryUpdateWallpaper_WhenExplicitBackgroundColorProvidedBeforeAnySuccessfulRefresh_SelectsInitialFont()
    {
        var quoteSelectionService = new SpyQuoteSelectionService();
        var monitorResolutionService = new SpyMonitorResolutionService();
        var wallpaperBackgroundColorService = new SpyWallpaperBackgroundColorService();
        var wallpaperFontSelectionService = new SpyWallpaperFontSelectionService();
        var wallpaperRenderService = new SpyWallpaperRenderService();
        var windowsWallpaperService = new SpyWindowsWallpaperService { ReturnValue = true };
        var sut = new WallpaperUpdateService(
            quoteSelectionService,
            monitorResolutionService,
            wallpaperBackgroundColorService,
            wallpaperFontSelectionService,
            wallpaperRenderService,
            windowsWallpaperService);
        var explicitBackgroundColor = Color.FromArgb(41, 55, 73);

        var result = sut.TryUpdateWallpaper([new Quote { Text = "Quote", Author = "Author" }], explicitBackgroundColor);

        Assert.True(result);
        Assert.Equal(1, quoteSelectionService.CallCount);
        Assert.Equal(0, wallpaperBackgroundColorService.GetNextAutomaticBackgroundColorCallCount);
        Assert.Equal(1, wallpaperFontSelectionService.CallCount);
        Assert.Equal(explicitBackgroundColor.ToArgb(), wallpaperRenderService.CapturedBackgroundColor.ToArgb());
        Assert.Equal(wallpaperFontSelectionService.NextFontFamilyName, wallpaperRenderService.CapturedFontFamilyName);
        Assert.Equal(explicitBackgroundColor.ToArgb(), wallpaperBackgroundColorService.CapturedSetCurrentBackgroundColor?.ToArgb());
    }

    [Fact]
    public void TryUpdateWallpaper_WhenExplicitBackgroundColorProvidedAfterSuccessfulRefresh_ReusesCurrentQuoteAndFont()
    {
        var initiallySelectedQuote = new Quote { Text = "First quote", Author = "Author One" };
        var laterCandidateQuote = new Quote { Text = "Second quote", Author = "Author Two" };
        var quoteSelectionService = new SequenceQuoteSelectionService(initiallySelectedQuote, laterCandidateQuote);
        var monitorResolutionService = new SpyMonitorResolutionService();
        var wallpaperBackgroundColorService = new SpyWallpaperBackgroundColorService();
        var wallpaperFontSelectionService = new SpyWallpaperFontSelectionService();
        var wallpaperRenderService = new SpyWallpaperRenderService();
        var windowsWallpaperService = new SpyWindowsWallpaperService { ReturnValue = true };
        var sut = new WallpaperUpdateService(
            quoteSelectionService,
            monitorResolutionService,
            wallpaperBackgroundColorService,
            wallpaperFontSelectionService,
            wallpaperRenderService,
            windowsWallpaperService);
        var explicitBackgroundColor = Color.FromArgb(35, 48, 61);

        var initialRefreshResult = sut.TryUpdateWallpaper([initiallySelectedQuote, laterCandidateQuote]);
        var colorOnlyRefreshResult = sut.TryUpdateWallpaper([initiallySelectedQuote, laterCandidateQuote], explicitBackgroundColor);

        Assert.True(initialRefreshResult);
        Assert.True(colorOnlyRefreshResult);
        Assert.Equal(1, quoteSelectionService.CallCount);
        Assert.Equal(1, wallpaperFontSelectionService.CallCount);
        Assert.Equal(2, wallpaperRenderService.CapturedQuotes.Count);
        Assert.Same(initiallySelectedQuote, wallpaperRenderService.CapturedQuotes[0]);
        Assert.Same(initiallySelectedQuote, wallpaperRenderService.CapturedQuotes[1]);
        Assert.Equal(
            [wallpaperFontSelectionService.NextFontFamilyName, wallpaperFontSelectionService.NextFontFamilyName],
            wallpaperRenderService.CapturedFontFamilyNames);
        Assert.Equal(explicitBackgroundColor.ToArgb(), wallpaperRenderService.CapturedBackgroundColors[1].ToArgb());
    }

    [Fact]
    public void TryUpdateWallpaper_WhenStandardRefreshRunsTwice_SelectsFontForEachRefresh()
    {
        var quoteSelectionService = new SpyQuoteSelectionService();
        var monitorResolutionService = new SpyMonitorResolutionService();
        var wallpaperBackgroundColorService = new SpyWallpaperBackgroundColorService();
        var wallpaperFontSelectionService = new SequenceWallpaperFontSelectionService("Georgia", "Segoe UI");
        var wallpaperRenderService = new SpyWallpaperRenderService();
        var windowsWallpaperService = new SpyWindowsWallpaperService { ReturnValue = true };
        var sut = new WallpaperUpdateService(
            quoteSelectionService,
            monitorResolutionService,
            wallpaperBackgroundColorService,
            wallpaperFontSelectionService,
            wallpaperRenderService,
            windowsWallpaperService);

        var firstRefreshResult = sut.TryUpdateWallpaper([new Quote { Text = "Quote", Author = "Author" }]);
        var secondRefreshResult = sut.TryUpdateWallpaper([new Quote { Text = "Quote", Author = "Author" }]);

        Assert.True(firstRefreshResult);
        Assert.True(secondRefreshResult);
        Assert.Equal(2, wallpaperFontSelectionService.CallCount);
        Assert.Equal(["Georgia", "Segoe UI"], wallpaperRenderService.CapturedFontFamilyNames);
    }

    [Fact]
    public void TryUpdateWallpaperWithRandomFont_WhenSuccessfulRefreshExists_ReusesCurrentQuoteAndBackgroundAndChangesFont()
    {
        var initiallySelectedQuote = new Quote { Text = "First quote", Author = "Author One" };
        var laterCandidateQuote = new Quote { Text = "Second quote", Author = "Author Two" };
        var quoteSelectionService = new SequenceQuoteSelectionService(initiallySelectedQuote, laterCandidateQuote);
        var monitorResolutionService = new SpyMonitorResolutionService();
        var wallpaperBackgroundColorService = new SpyWallpaperBackgroundColorService();
        var wallpaperFontSelectionService = new SpyWallpaperFontSelectionService();
        var wallpaperRenderService = new SpyWallpaperRenderService();
        var windowsWallpaperService = new SpyWindowsWallpaperService { ReturnValue = true };
        var sut = new WallpaperUpdateService(
            quoteSelectionService,
            monitorResolutionService,
            wallpaperBackgroundColorService,
            wallpaperFontSelectionService,
            wallpaperRenderService,
            windowsWallpaperService);

        var initialRefreshResult = sut.TryUpdateWallpaper([initiallySelectedQuote, laterCandidateQuote]);
        var randomFontRefreshResult = sut.TryUpdateWallpaperWithRandomFont([initiallySelectedQuote, laterCandidateQuote]);

        Assert.True(initialRefreshResult);
        Assert.True(randomFontRefreshResult);
        Assert.Equal(1, quoteSelectionService.CallCount);
        Assert.Equal(1, wallpaperBackgroundColorService.GetCurrentBackgroundColorCallCount);
        Assert.Equal(1, wallpaperBackgroundColorService.GetNextAutomaticBackgroundColorCallCount);
        Assert.Equal(1, wallpaperFontSelectionService.CallCount);
        Assert.Equal(1, wallpaperFontSelectionService.ExcludingCurrentFontCallCount);
        Assert.Equal(wallpaperFontSelectionService.NextFontFamilyName, wallpaperFontSelectionService.ExcludedFontFamilyName);
        Assert.Equal(2, wallpaperRenderService.CapturedQuotes.Count);
        Assert.Same(initiallySelectedQuote, wallpaperRenderService.CapturedQuotes[0]);
        Assert.Same(initiallySelectedQuote, wallpaperRenderService.CapturedQuotes[1]);
        Assert.Equal(2, wallpaperRenderService.CapturedBackgroundColors.Count);
        Assert.Equal(wallpaperBackgroundColorService.NextAutomaticBackgroundColor.ToArgb(), wallpaperRenderService.CapturedBackgroundColors[0].ToArgb());
        Assert.Equal(wallpaperBackgroundColorService.CurrentBackgroundColor.ToArgb(), wallpaperRenderService.CapturedBackgroundColors[1].ToArgb());
        Assert.Equal(
            [wallpaperFontSelectionService.NextFontFamilyName, wallpaperFontSelectionService.NextDifferentFontFamilyName],
            wallpaperRenderService.CapturedFontFamilyNames);
    }

    [Fact]
    public void TryUpdateWallpaperWithRandomFont_WhenNoSuccessfulRefreshExists_UsesCurrentBackgroundFallbackAndSelectsQuote()
    {
        var quoteSelectionService = new SpyQuoteSelectionService();
        var monitorResolutionService = new SpyMonitorResolutionService();
        var wallpaperBackgroundColorService = new SpyWallpaperBackgroundColorService();
        var wallpaperFontSelectionService = new SpyWallpaperFontSelectionService();
        var wallpaperRenderService = new SpyWallpaperRenderService();
        var windowsWallpaperService = new SpyWindowsWallpaperService { ReturnValue = true };
        var sut = new WallpaperUpdateService(
            quoteSelectionService,
            monitorResolutionService,
            wallpaperBackgroundColorService,
            wallpaperFontSelectionService,
            wallpaperRenderService,
            windowsWallpaperService);
        var quote = new Quote { Text = "Quote", Author = "Author" };

        var result = sut.TryUpdateWallpaperWithRandomFont([quote]);

        Assert.True(result);
        Assert.Equal(1, quoteSelectionService.CallCount);
        Assert.Equal(1, wallpaperBackgroundColorService.GetCurrentBackgroundColorCallCount);
        Assert.Equal(0, wallpaperBackgroundColorService.GetNextAutomaticBackgroundColorCallCount);
        Assert.Equal(0, wallpaperFontSelectionService.CallCount);
        Assert.Equal(1, wallpaperFontSelectionService.ExcludingCurrentFontCallCount);
        Assert.Null(wallpaperFontSelectionService.ExcludedFontFamilyName);
        Assert.Same(quote, wallpaperRenderService.CapturedQuote);
        Assert.Equal(wallpaperBackgroundColorService.CurrentBackgroundColor.ToArgb(), wallpaperRenderService.CapturedBackgroundColor.ToArgb());
        Assert.Equal(wallpaperFontSelectionService.NextDifferentFontFamilyName, wallpaperRenderService.CapturedFontFamilyName);
    }

    #endregion

    #region Negative cases

    [Fact]
    public void TryUpdateWallpaper_WhenNoQuoteCanBeSelected_ReturnsFalseAndSkipsDependencies()
    {
        var quoteSelectionService = new SpyQuoteSelectionService { ReturnValue = false };
        var monitorResolutionService = new SpyMonitorResolutionService();
        var wallpaperBackgroundColorService = new SpyWallpaperBackgroundColorService();
        var wallpaperFontSelectionService = new SpyWallpaperFontSelectionService();
        var wallpaperRenderService = new SpyWallpaperRenderService();
        var windowsWallpaperService = new SpyWindowsWallpaperService();
        var sut = new WallpaperUpdateService(
            quoteSelectionService,
            monitorResolutionService,
            wallpaperBackgroundColorService,
            wallpaperFontSelectionService,
            wallpaperRenderService,
            windowsWallpaperService);

        var result = sut.TryUpdateWallpaper([]);

        Assert.False(result);
        Assert.Equal(1, quoteSelectionService.CallCount);
        Assert.Equal(0, monitorResolutionService.CallCount);
        Assert.Equal(0, wallpaperRenderService.CallCount);
        Assert.Equal(0, windowsWallpaperService.CallCount);
        Assert.Equal(0, wallpaperBackgroundColorService.GetNextAutomaticBackgroundColorCallCount);
        Assert.Equal(0, wallpaperBackgroundColorService.SetCurrentBackgroundColorCallCount);
        Assert.Equal(0, wallpaperFontSelectionService.CallCount);
    }

    [Fact]
    public void TryUpdateWallpaper_WhenWallpaperRenderingThrows_ReturnsFalse()
    {
        var quoteSelectionService = new SpyQuoteSelectionService();
        var monitorResolutionService = new SpyMonitorResolutionService();
        var wallpaperBackgroundColorService = new SpyWallpaperBackgroundColorService();
        var wallpaperFontSelectionService = new SpyWallpaperFontSelectionService();
        var wallpaperRenderService = new SpyWallpaperRenderService { ThrowOnRender = true };
        var windowsWallpaperService = new SpyWindowsWallpaperService();
        var sut = new WallpaperUpdateService(
            quoteSelectionService,
            monitorResolutionService,
            wallpaperBackgroundColorService,
            wallpaperFontSelectionService,
            wallpaperRenderService,
            windowsWallpaperService);

        var result = sut.TryUpdateWallpaper([new Quote { Text = "Quote", Author = "Author" }]);

        Assert.False(result);
        Assert.Equal(1, quoteSelectionService.CallCount);
        Assert.Equal(1, monitorResolutionService.CallCount);
        Assert.Equal(1, wallpaperRenderService.CallCount);
        Assert.Equal(0, windowsWallpaperService.CallCount);
        Assert.Equal(1, wallpaperBackgroundColorService.GetNextAutomaticBackgroundColorCallCount);
        Assert.Equal(0, wallpaperBackgroundColorService.SetCurrentBackgroundColorCallCount);
        Assert.Equal(1, wallpaperFontSelectionService.CallCount);
    }

    [Fact]
    public void TryUpdateWallpaper_WhenWallpaperApplyFails_ReturnsFalse()
    {
        var quoteSelectionService = new SpyQuoteSelectionService();
        var monitorResolutionService = new SpyMonitorResolutionService();
        var wallpaperBackgroundColorService = new SpyWallpaperBackgroundColorService();
        var wallpaperFontSelectionService = new SpyWallpaperFontSelectionService();
        var wallpaperRenderService = new SpyWallpaperRenderService();
        var windowsWallpaperService = new SpyWindowsWallpaperService { ReturnValue = false };
        var sut = new WallpaperUpdateService(
            quoteSelectionService,
            monitorResolutionService,
            wallpaperBackgroundColorService,
            wallpaperFontSelectionService,
            wallpaperRenderService,
            windowsWallpaperService);

        var result = sut.TryUpdateWallpaper([new Quote { Text = "Quote", Author = "Author" }]);

        Assert.False(result);
        Assert.Equal(1, quoteSelectionService.CallCount);
        Assert.Equal(1, monitorResolutionService.CallCount);
        Assert.Equal(1, wallpaperRenderService.CallCount);
        Assert.Equal(1, windowsWallpaperService.CallCount);
        Assert.Equal(1, wallpaperBackgroundColorService.GetNextAutomaticBackgroundColorCallCount);
        Assert.Equal(0, wallpaperBackgroundColorService.SetCurrentBackgroundColorCallCount);
        Assert.Equal(1, wallpaperFontSelectionService.CallCount);
    }

    #endregion
}