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

    public bool TryUpdateWallpaper(IEnumerable<Quote>? configuredQuotes, Color? backgroundColor = null)
    {
        var isBackgroundColorOnlyRefresh = backgroundColor.HasValue;
        var selectedQuote = isBackgroundColorOnlyRefresh
            ? _currentQuote
            : null;
        var selectedFontFamilyName = isBackgroundColorOnlyRefresh
            ? _currentFontFamilyName
            : null;

        if (selectedQuote is null &&
            (!_quoteSelectionService.TrySelectQuote(configuredQuotes, out selectedQuote) || selectedQuote is null))
            return false;

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

            return wallpaperApplied;
        }
        catch (ArgumentException)
        {
            return false;
        }
        catch (IOException)
        {
            return false;
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }
        catch (ExternalException)
        {
            return false;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }
}