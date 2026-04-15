namespace DeskQuotes.Services.Implementations;

public class WallpaperUpdateService(
    QuoteSelectionService quoteSelectionService,
    MonitorResolutionService monitorResolutionService,
    WallpaperBackgroundColorService wallpaperBackgroundColorService,
    WallpaperFontSelectionService wallpaperFontSelectionService,
    WallpaperRenderService wallpaperRenderService,
    WindowsWallpaperService windowsWallpaperService)
{
    private readonly MonitorResolutionService _monitorResolutionService = monitorResolutionService ?? throw new ArgumentNullException(nameof(monitorResolutionService));
    private readonly QuoteSelectionService _quoteSelectionService = quoteSelectionService ?? throw new ArgumentNullException(nameof(quoteSelectionService));

    private readonly WallpaperBackgroundColorService _wallpaperBackgroundColorService =
        wallpaperBackgroundColorService ?? throw new ArgumentNullException(nameof(wallpaperBackgroundColorService));

    private readonly WallpaperFontSelectionService _wallpaperFontSelectionService =
        wallpaperFontSelectionService ?? throw new ArgumentNullException(nameof(wallpaperFontSelectionService));

    private readonly WallpaperRenderService _wallpaperRenderService = wallpaperRenderService ?? throw new ArgumentNullException(nameof(wallpaperRenderService));
    private readonly WindowsWallpaperService _windowsWallpaperService = windowsWallpaperService ?? throw new ArgumentNullException(nameof(windowsWallpaperService));
    private string? _currentFontFamilyName;
    private Quote? _currentQuote;

    public string? GetCurrentFontFamilyName()
    {
        return _currentFontFamilyName;
    }

    public bool TryUpdateWallpaper(IEnumerable<Quote>? configuredQuotes, Color? backgroundColor = null)
    {
        return UpdateWallpaper(configuredQuotes, null, backgroundColor) == WallpaperUpdateResult.Success;
    }

    public WallpaperUpdateResult UpdateWallpaper(IEnumerable<Quote>? configuredQuotes, string? selectedMood = null, Color? backgroundColor = null)
    {
        return TryUpdateWallpaperCore(
            configuredQuotes,
            selectedMood,
            backgroundColor,
            null,
            backgroundColor.HasValue,
            backgroundColor.HasValue);
    }

    public bool TryUpdateWallpaperWithRandomFont(IEnumerable<Quote>? configuredQuotes)
    {
        return UpdateWallpaperWithRandomFont(configuredQuotes) == WallpaperUpdateResult.Success;
    }

    public WallpaperUpdateResult UpdateWallpaperWithRandomFont(IEnumerable<Quote>? configuredQuotes, string? selectedMood = null)
    {
        return TryUpdateWallpaperCore(
            configuredQuotes,
            selectedMood,
            _wallpaperBackgroundColorService.GetCurrentBackgroundColor(),
            _wallpaperFontSelectionService.GetRandomFontFamilyName(_currentFontFamilyName),
            true,
            false);
    }

    private WallpaperUpdateResult TryUpdateWallpaperCore(
        IEnumerable<Quote>? configuredQuotes,
        string? selectedMood,
        Color? backgroundColor,
        string? fontFamilyName,
        bool reuseCurrentQuote,
        bool reuseCurrentFont)
    {
        var normalizedSelectedMood = NormalizeMood(selectedMood);
        var filteredQuotes = FilterQuotesByMood(configuredQuotes, normalizedSelectedMood);
        var selectedQuote = reuseCurrentQuote && QuoteMatchesMood(_currentQuote, normalizedSelectedMood)
            ? _currentQuote
            : null;
        var selectedFontFamilyName = reuseCurrentFont
            ? _currentFontFamilyName
            : fontFamilyName;

        if (selectedQuote is null)
        {
            if (normalizedSelectedMood is not null && filteredQuotes.Length == 0)
                return WallpaperUpdateResult.NoMatchingQuotesForSelectedMood;

            if (!_quoteSelectionService.TrySelectQuote(filteredQuotes, out selectedQuote) || selectedQuote is null)
                return WallpaperUpdateResult.Failed;
        }

        selectedFontFamilyName ??= _wallpaperFontSelectionService.GetRandomFontFamilyName();

        try
        {
            var resolution = _monitorResolutionService.InferWallpaperResolution();
            var selectedBackgroundColor = backgroundColor ?? _wallpaperBackgroundColorService.GetNextAutomaticBackgroundColor();
            var wallpaperPath = _wallpaperRenderService.RenderQuoteWallpaper(selectedQuote, resolution, selectedBackgroundColor, selectedFontFamilyName);
            var wallpaperApplied = _windowsWallpaperService.TryApplyWallpaper(wallpaperPath);

            if (wallpaperApplied)
            {
                _currentQuote = selectedQuote;
                _currentFontFamilyName = selectedFontFamilyName;
                _wallpaperBackgroundColorService.SetCurrentBackgroundColor(selectedBackgroundColor);
            }

            return wallpaperApplied
                ? WallpaperUpdateResult.Success
                : WallpaperUpdateResult.Failed;
        }
        catch (ArgumentException)
        {
            return WallpaperUpdateResult.Failed;
        }
        catch (IOException)
        {
            return WallpaperUpdateResult.Failed;
        }
        catch (UnauthorizedAccessException)
        {
            return WallpaperUpdateResult.Failed;
        }
        catch (ExternalException)
        {
            return WallpaperUpdateResult.Failed;
        }
        catch (InvalidOperationException)
        {
            return WallpaperUpdateResult.Failed;
        }
    }

    private static Quote[] FilterQuotesByMood(IEnumerable<Quote>? configuredQuotes, string? selectedMood)
    {
        var configuredQuotesArray = configuredQuotes?.ToArray() ?? [];
        if (selectedMood is null)
            return configuredQuotesArray;

        return configuredQuotesArray
            .Where(quote => QuoteMatchesMood(quote, selectedMood))
            .ToArray();
    }

    private static bool QuoteMatchesMood(Quote? quote, string? selectedMood)
    {
        if (quote is null)
            return false;

        if (selectedMood is null)
            return true;

        return quote.Tags?.Any(tag =>
                   !string.IsNullOrWhiteSpace(tag) &&
                   string.Equals(tag.Trim(), selectedMood, StringComparison.OrdinalIgnoreCase))
               == true;
    }

    private static string? NormalizeMood(string? selectedMood)
    {
        return string.IsNullOrWhiteSpace(selectedMood)
            ? null
            : selectedMood.Trim();
    }
}
